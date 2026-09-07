# Dynamic Wizard untuk Requisition (Item Request)

> Status: **Proposal / Draft implementasi**
> Modul terdampak: `Requisition`, `RequisitionDetail` (PupaAPI / .NET + EF Core + PostgreSQL)
> Referensi konsep: **BeesuiteGO** — "pre-inspection checklist job"
> Tujuan: mengganti puluhan kolom form yang di-*hardcode* per jenis barang (`Freon*`, `WireRope*`, `InnerDiameter`, `Brand`, `Model`, `MotorType`, …) dengan **form dinamis berbasis template** yang dirender sebagai **wizard bertahap** di frontend — tambah field/step baru **tanpa `ALTER TABLE`** dan tanpa ubah `BusinessObject`.
>
> **Keputusan desain:** Requisition memakai **tabel sendiri** (tidak *reuse* `JobFieldDefinition`/`JobFieldValue` milik modul Job). Tiga tabel:
> `RequisitionDynamicFormTemplate` · `RequisitionDynamicForm` · `RequisitionDynamicFormData`.

---

## 1. Bagaimana BeesuiteGO melakukannya

BeesuiteGO memakai **dua lapis** yang saling melengkapi.

### 1a. Blueprint field generik — `JobFieldDefinition` + `JobFieldValue` (EAV) + adapter `AdditionalData`

| Komponen | Peran |
|---|---|
| `JobFieldDefinition` (tabel *read-only*) | Cetak biru form: `FormType`, `FieldKey`, `FieldLabel`, `FieldType`, `IsRequired`, `IsVisible`, `IsReadOnly`, `Options` (jsonb), `SortOrder`. Frontend GET daftar ini untuk **merender form**. |
| `JobFieldValue` (tabel EAV polimorfik) | Nilai per record: `EntityType` (`Job`\|`JobRequest`), `EntityID`, `FieldKey`, `ValueType` (`text`\|`number`\|`date`\|`bool`\|`json`\|`null`), plus kolom nilai paralel `ValueText/ValueNumber/ValueDate/ValueBool/ValueJson`. |
| Adapter di service (`job_field_value.go`) | Kontrak API tetap: klien kirim/terima **`AdditionalData` sebagai object JSON**. Saat **tulis** → object dipecah jadi baris EAV. Saat **baca** → baris dirakit ulang jadi object yang *byte-equivalent*. |

Alasan desain (dari `docs/MIGRATION_AdditionalData.md`):
- Field form memang **dinamis per `FormType`** → kolom flat membengkak & banyak NULL (persis kondisi `RequisitionDetail` sekarang).
- EAV = tambah field cukup **tambah baris**, tanpa DDL dari input user.
- Kolom nilai **bertipe paralel** (bukan satu kolom string) supaya agregasi numerik/tanggal untuk laporan tetap benar & cepat.
- `ValueJson` = *fallback lossless* untuk nilai bersarang (object/array) → round-trip tidak merusak response.
- Riwayat per-field bisa dibangun karena perubahan tercatat per `FieldKey`, bukan per blob.

### 1b. Wizard per-item — `RepairEquipmentItems.StepConfigJson`

Kolom `jsonb` `StepConfigJson` pada master `RepairEquipmentItems` (per `ItemCode`) menyimpan **konfigurasi wizard**: urutan **step → sequence → field**, `visibleForJobTypes`, dan `postSubmitItems` (barang yang di-*auto-request* dari jawaban checklist). Jawaban crew disimpan sebagai **StepValues** per Job, lalu di-*resolve* server-side (`auto_ir_service.go`) menjadi Item Request otomatis.

Intinya: **satu master row membawa "bentuk" wizard-nya sendiri**; dokumen hanya menyimpan jawaban.

---

## 2. Desain untuk Requisition — 3 tabel sendiri

```
RequisitionDynamicFormTemplate   (blueprint/wizard — dikelola admin, versioned)
        │  1
        │  *
RequisitionDynamicForm           (1 instance per Requisition / RequisitionDetail)
        │  1
        │  *
RequisitionDynamicFormData       (nilai per field — EAV, kolom nilai paralel)
```

| Tabel | Analog BeesuiteGO | Isi |
|---|---|---|
| **`RequisitionDynamicFormTemplate`** | `RepairEquipmentItems.StepConfigJson` + `JobFieldDefinition` digabung | Definisi form/wizard: metadata + `SchemaJson` (steps → fields, kondisi, validasi, prefill, postSubmitItems). Di-*scope* per `CategoryID` / `StockFamily` / `ItemType` / global. **Versioned** — template yang sudah dipakai tidak diubah, dibuat versi baru. |
| **`RequisitionDynamicForm`** | (baru — tidak ada padanan langsung) | Menautkan **satu** `Requisition` **atau** satu `RequisitionDetail` ke **satu** template + **snapshot** versi/schema yang dipakai saat itu. Jadi anchor untuk data & cascade. |
| **`RequisitionDynamicFormData`** | `JobFieldValue` / `StepValues` | Nilai jawaban: `FormID` + `StepKey` + `FieldKey` + `ValueType` + kolom nilai paralel. |

Kenapa `RequisitionDynamicForm` di tengah (bukan langsung Template→Data seperti EAV BeesuiteGO):
- **FK tegas + `ON DELETE CASCADE`** — hapus Requisition → Form ikut → Data ikut, otomatis oleh DB (BeesuiteGO harus cascade manual karena polimorfik).
- **Snapshot schema** — kalau admin menerbitkan template versi baru, dokumen lama tetap dirender & divalidasi dengan schema yang berlaku saat dibuat.
- Satu tempat untuk status pengisian wizard (`Draft`/`Completed`) & audit `CreatedBy/At`.

`EntityType` (`Requisition` | `RequisitionDetail`) tetap dipakai supaya header dan tiap baris item bisa punya wizard-nya sendiri — sama seperti `Job` vs `JobRequest` di BeesuiteGO.

---

## 3. Skema database (PostgreSQL — dikelola manual, ikuti konvensi `migration_*.sql`)

```sql
-- migration_RequisitionDynamicForm.sql
BEGIN;

-- 3.1  TEMPLATE (blueprint wizard, versioned) --------------------------------
CREATE TABLE IF NOT EXISTS "RequisitionDynamicFormTemplate" (
    "ID"           integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "Code"         varchar(100) NOT NULL,               -- "FREON", "WIRE_ROPE", "DEFAULT"
    "Name"         varchar(255) NOT NULL,
    "Version"      integer      NOT NULL DEFAULT 1,
    "Scope"        varchar(30)  NOT NULL DEFAULT 'Category',  -- Category | Family | ItemType | Global
    "CategoryID"   integer NULL,                         -- FK StockFamily.ID (Scope=Category/Family)
    "ItemType"     integer NULL,                         -- Scope=ItemType
    "EntityType"   varchar(30)  NOT NULL DEFAULT 'RequisitionDetail', -- Requisition | RequisitionDetail
    "IsActive"     boolean      NOT NULL DEFAULT true,
    "IsPublished"  boolean      NOT NULL DEFAULT false,  -- draft admin vs siap dipakai
    "SchemaJson"   jsonb        NOT NULL DEFAULT '{}',   -- lihat §5
    "CreatedAt"    timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"    timestamptz  NOT NULL DEFAULT now(),
    "CreatedBy"    varchar(100) NULL,
    CONSTRAINT "uq_reqdynformtpl_code_ver" UNIQUE ("Code", "Version")
);
CREATE INDEX IF NOT EXISTS "idx_reqdynformtpl_lookup"
    ON "RequisitionDynamicFormTemplate" ("EntityType","Scope","CategoryID","ItemType","IsActive","IsPublished");

-- 3.2  FORM (instance per Requisition / RequisitionDetail) -------------------
CREATE TABLE IF NOT EXISTS "RequisitionDynamicForm" (
    "ID"              integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "RequisitionID"   integer      NOT NULL
        REFERENCES "Requisition"("ID") ON DELETE CASCADE,
    "EntityType"      varchar(30)  NOT NULL,             -- Requisition | RequisitionDetail
    "EntityID"        integer      NOT NULL,             -- Requisition.ID atau RequisitionDetail.ID
    "TemplateID"      integer      NOT NULL
        REFERENCES "RequisitionDynamicFormTemplate"("ID"),
    "TemplateCode"    varchar(100) NOT NULL,
    "TemplateVersion" integer      NOT NULL,
    "SchemaSnapshot"  jsonb        NOT NULL,             -- salinan SchemaJson saat form dibuat
    "Status"          varchar(20)  NOT NULL DEFAULT 'Draft', -- Draft | Completed
    "CreatedAt"       timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"       timestamptz  NOT NULL DEFAULT now(),
    "CreatedBy"       varchar(100) NULL,
    CONSTRAINT "uq_reqdynform_entity" UNIQUE ("EntityType","EntityID")
);
CREATE INDEX IF NOT EXISTS "idx_reqdynform_requisition" ON "RequisitionDynamicForm" ("RequisitionID");

-- 3.3  DATA (nilai per field — EAV, kolom nilai paralel) --------------------
CREATE TABLE IF NOT EXISTS "RequisitionDynamicFormData" (
    "ID"          integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "FormID"      integer      NOT NULL
        REFERENCES "RequisitionDynamicForm"("ID") ON DELETE CASCADE,
    "StepKey"     varchar(100) NULL,
    "FieldKey"    varchar(100) NOT NULL,
    "ValueType"   varchar(20)  NOT NULL DEFAULT 'text',  -- text|number|date|bool|json|null
    "ValueText"   text         NULL,
    "ValueNumber" numeric      NULL,
    "ValueDate"   date         NULL,
    "ValueBool"   boolean      NULL,
    "ValueJson"   jsonb        NULL,
    "CreatedAt"   timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"   timestamptz  NOT NULL DEFAULT now(),
    "CreatedBy"   varchar(100) NULL,
    "UpdatedBy"   varchar(100) NULL,
    CONSTRAINT "uq_reqdynformdata_field" UNIQUE ("FormID","FieldKey")
);
CREATE INDEX IF NOT EXISTS "idx_reqdynformdata_form"     ON "RequisitionDynamicFormData" ("FormID");
CREATE INDEX IF NOT EXISTS "idx_reqdynformdata_key_num"  ON "RequisitionDynamicFormData" ("FieldKey","ValueNumber");

-- 3.4  (opsional) history per-field untuk akumulasi time-series -------------
CREATE TABLE IF NOT EXISTS "RequisitionDynamicFormDataHistory" (
    "ID"        integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "FormID"    integer      NOT NULL,
    "FieldKey"  varchar(100) NOT NULL,
    "OldValue"  text,
    "NewValue"  text,
    "ChangedBy" varchar(100),
    "ChangedAt" timestamptz  NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS "idx_reqdynformdatahist_form" ON "RequisitionDynamicFormDataHistory" ("FormID","FieldKey","ChangedAt");

COMMIT;
```

Catatan:
- **`RequisitionID` selalu diisi** walau `EntityType='RequisitionDetail'` (ambil dari `RequisitionDetail.RequisitionID`) — supaya cascade & query "semua form milik requisition ini" sederhana.
- `uq_reqdynform_entity` menjamin **satu form per entity**. Kalau butuh multi-form per entity (mis. beberapa wizard berbeda pada satu detail), ganti jadi `UNIQUE ("EntityType","EntityID","TemplateCode")`.
- Kolom nilai **bertipe paralel** — sama alasannya seperti BeesuiteGO (agregasi numerik/tanggal benar; `ValueJson` fallback lossless).

---

## 4. BusinessObjects (C#) + DbContext

```csharp
// Pupa/BusinessObjects/Beesuite/RequisitionDynamicFormTemplate.cs
[Table("RequisitionDynamicFormTemplate")]
public class RequisitionDynamicFormTemplate : BaseEntity
{
    [Key] public virtual int ID { get; set; }
    [Required, StringLength(100)] public virtual string Code { get; set; } = "";
    [Required, StringLength(255)] public virtual string Name { get; set; } = "";
    public virtual int Version { get; set; } = 1;
    [Required, StringLength(30)]  public virtual string Scope { get; set; } = "Category";
    public virtual int? CategoryID { get; set; }
    public virtual int? ItemType { get; set; }
    [Required, StringLength(30)]  public virtual string EntityType { get; set; } = "RequisitionDetail";
    public virtual bool IsActive { get; set; } = true;
    public virtual bool IsPublished { get; set; }
    [Column(TypeName = "jsonb")] public virtual string SchemaJson { get; set; } = "{}";
    public virtual DateTime? CreatedAt { get; set; } = DateTime.Now;
    public virtual DateTime? UpdatedAt { get; set; } = DateTime.Now;
    [StringLength(100)] public virtual string? CreatedBy { get; set; }

    [ForeignKey("CategoryID")] public virtual StockFamily? Category { get; set; }
}

// Pupa/BusinessObjects/Beesuite/RequisitionDynamicForm.cs
[Table("RequisitionDynamicForm")]
public class RequisitionDynamicForm : BaseEntity
{
    public RequisitionDynamicForm() { Data = new ObservableCollection<RequisitionDynamicFormData>(); }

    [Key] public virtual int ID { get; set; }
    public virtual int RequisitionID { get; set; }
    [Required, StringLength(30)]  public virtual string EntityType { get; set; } = "";
    public virtual int EntityID { get; set; }
    public virtual int TemplateID { get; set; }
    [Required, StringLength(100)] public virtual string TemplateCode { get; set; } = "";
    public virtual int TemplateVersion { get; set; }
    [Column(TypeName = "jsonb")] public virtual string SchemaSnapshot { get; set; } = "{}";
    [Required, StringLength(20)]  public virtual string Status { get; set; } = "Draft";
    public virtual DateTime? CreatedAt { get; set; } = DateTime.Now;
    public virtual DateTime? UpdatedAt { get; set; } = DateTime.Now;
    [StringLength(100)] public virtual string? CreatedBy { get; set; }

    [ForeignKey("RequisitionID")] public virtual Requisition? Requisition { get; set; }
    [ForeignKey("TemplateID")]    public virtual RequisitionDynamicFormTemplate? Template { get; set; }
    public virtual ObservableCollection<RequisitionDynamicFormData> Data { get; set; }
}

// Pupa/BusinessObjects/Beesuite/RequisitionDynamicFormData.cs
[Table("RequisitionDynamicFormData")]
public class RequisitionDynamicFormData : BaseEntity
{
    [Key] public virtual int ID { get; set; }
    public virtual int FormID { get; set; }
    [StringLength(100)] public virtual string? StepKey { get; set; }
    [Required, StringLength(100)] public virtual string FieldKey { get; set; } = "";
    [Required, StringLength(20)]  public virtual string ValueType { get; set; } = "text";
    public virtual string?   ValueText { get; set; }
    public virtual decimal?  ValueNumber { get; set; }
    public virtual DateTime? ValueDate { get; set; }
    public virtual bool?     ValueBool { get; set; }
    [Column(TypeName = "jsonb")] public virtual string? ValueJson { get; set; }
    public virtual DateTime? CreatedAt { get; set; } = DateTime.Now;
    public virtual DateTime? UpdatedAt { get; set; } = DateTime.Now;
    [StringLength(100)] public virtual string? CreatedBy { get; set; }
    [StringLength(100)] public virtual string? UpdatedBy { get; set; }

    [ForeignKey("FormID")] public virtual RequisitionDynamicForm? Form { get; set; }
}
```

`BeesuiteDbContext.cs`:

```csharp
public DbSet<RequisitionDynamicFormTemplate> RequisitionDynamicFormTemplate { get; set; }
public DbSet<RequisitionDynamicForm>         RequisitionDynamicForm { get; set; }
public DbSet<RequisitionDynamicFormData>     RequisitionDynamicFormData { get; set; }

// OnModelCreating
modelBuilder.Entity<RequisitionDynamicForm>()
    .HasMany(f => f.Data).WithOne(d => d.Form)
    .HasForeignKey(d => d.FormID).OnDelete(DeleteBehavior.Cascade);
modelBuilder.Entity<RequisitionDynamicForm>()
    .HasOne(f => f.Requisition).WithMany()
    .HasForeignKey(f => f.RequisitionID).OnDelete(DeleteBehavior.Cascade);
modelBuilder.Entity<RequisitionDynamicForm>()
    .HasIndex(f => new { f.EntityType, f.EntityID }).IsUnique();
```

Properti transient di header/detail (untuk kontrak API — **tanpa kolom baru**):

```csharp
// Requisition.cs & RequisitionDetail.cs
[NotMapped] public virtual JsonElement? DynamicForm { get; set; }
// bentuk: { "templateCode": "...", "templateVersion": 3, "values": { "FieldKey": <value>, ... } }
```

---

## 5. Bentuk `SchemaJson` (kontrak template/wizard)

Dikembangkan dari `StepConfigJson` BeesuiteGO. `field.key` = `RequisitionDynamicFormData.FieldKey`, `step.key` = `StepKey`.

```jsonc
{
  "version": 3,
  "title": "Permintaan Wire Rope",
  "entityType": "RequisitionDetail",
  "appliesTo": { "scope": "Category", "categoryId": 42 },

  "steps": [
    {
      "key": "identifikasi",
      "title": "Identifikasi Barang",
      "visibleWhen": null,
      "fields": [
        { "key": "Brand",   "label": "Merek",  "type": "text",   "required": true },
        { "key": "Model",   "label": "Model",  "type": "text" },
        { "key": "EndType", "label": "Tipe Ujung", "type": "dropdown",
          "options": ["Eye", "Thimble", "Socket"], "required": true }
      ]
    },
    {
      "key": "dimensi",
      "title": "Dimensi",
      "visibleWhen": "identifikasi.EndType == 'Eye'",
      "fields": [
        { "key": "InnerDiameter", "label": "Ø Dalam (mm)", "type": "number", "min": 0 },
        { "key": "OuterDiameter", "label": "Ø Luar (mm)",  "type": "number", "min": 0 },
        { "key": "EyeLengthM",    "label": "Panjang Mata (m)", "type": "number", "required": true }
      ]
    },
    {
      "key": "evaluasi",
      "title": "Evaluasi & Lampiran",
      "fields": [
        { "key": "ReplacementReason", "label": "Alasan Penggantian", "type": "textarea", "required": true },
        { "key": "DamageReport", "label": "Laporan Kerusakan", "type": "attachment",
          "requiredWhen": "evaluasi.ReplacementReason != ''" }
      ]
    }
  ],

  "prefill": [
    { "target": "identifikasi.Brand", "from": "item.Brand" }
  ],

  "validation": [
    { "rule": "dimensi.InnerDiameter < dimensi.OuterDiameter",
      "message": "Ø dalam harus lebih kecil dari Ø luar" }
  ],

  "postSubmitItems": [
    { "itemId": 10231, "familyId": 42, "qtyFromStep": "dimensi", "qtyFromField": "RollQty" }
  ]
}
```

Tipe field: `text`, `textarea`, `number`, `date`, `boolean`, `dropdown`, `multiselect`, `attachment`, `grid`.

Aturan:
- **`step.key` & `field.key` stabil** — jangan di-rename setelah dipakai (= kehilangan tautan histori). Rename = buat `Version` baru template.
- `visibleWhen`/`requiredWhen`/`validation` = ekspresi sederhana `stepKey.fieldKey <op> nilai` dengan `&& || == != < >`. Dievaluasi di **frontend** (UX) **dan** divalidasi ulang di **backend** saat simpan.
- `attachment` tetap lewat `RequisitionDetailAttachmentRel`; simpan `attachmentId`/path di `ValueJson`.

---

## 6. Perubahan backend (PupaAPI)

### 6.1 Adapter nilai ⇄ tabel Data — `Services/RequisitionDynamicFormService.cs`

Port logika dari `job_field_value.go`, tapi target tabel `RequisitionDynamicFormData` via `FormID`:

```csharp
public class RequisitionDynamicFormService
{
    // Pilih template yang berlaku (paling spesifik menang): ItemType -> Category/Family -> Global.
    Task<RequisitionDynamicFormTemplate?> ResolveTemplateAsync(
        string entityType, int? categoryId, int? itemType);

    // object {templateCode, values{}} -> RequisitionDynamicForm + baris Data.
    //   number->ValueNumber, bool->ValueBool, string->ValueText
    //   (atau ValueDate bila field.type=date & format valid), object/array->ValueJson, null->ValueType="null".
    Task UpsertAsync(BeesuiteDbContext db, string entityType, int entityId, int requisitionId,
                     JsonElement? dynamicForm, string? actor);
    //   1) resolve/attach template, isi SchemaSnapshot bila Form baru
    //   2) hapus baris Data lama utk FormID (full replace) — atau merge kalau values partial
    //   3) insert baris Data hasil decode
    //   4) validasi required/requiredWhen/validation atas SchemaSnapshot -> throw 400 bila gagal
    //   5) (opsional) tulis RequisitionDynamicFormDataHistory per field berubah
    //   6) set Form.Status = "Completed" bila semua required terisi

    // FormID(s) -> object { templateCode, templateVersion, values{} } byte-friendly utk response.
    Task<Dictionary<(string,int), JsonObject>> LoadAsync(
        BeesuiteDbContext db, IEnumerable<(string entityType,int entityId)> keys);
}
```

### 6.2 Endpoint

| Endpoint | Guna |
|---|---|
| `GET beesuite/api/RequisitionDynamicFormTemplate?entityType=RequisitionDetail&categoryId=42&itemType=3` | Resolusi template aktif+published untuk frontend render wizard. Kosong → form default / tanpa wizard. |
| `GET beesuite/api/RequisitionDynamicFormTemplate/{code}` (+ `?version=`) | Ambil satu template (default: versi published tertinggi). |
| `POST/PUT beesuite/api/RequisitionDynamicFormTemplate` | Admin CRUD blueprint. PUT pada template yang **sudah dipakai** → tolak; buat `Version` baru (`IsPublished=false` sampai diterbitkan). |
| `GET beesuite/api/Requisition/{id}/dynamic-form` | Semua `RequisitionDynamicForm` + `Data` milik requisition (header + tiap detail), sudah dirakit jadi object `values`. |

### 6.3 Wire ke `RequisitionController`

- **`CreateRequisition` / update** — dalam **satu `IDbContextTransaction`**: setelah `SaveChangesAsync()` (Requisition + Details punya ID), untuk header (bila `Body.DynamicForm` ada) dan tiap detail (`d.DynamicForm`), panggil `UpsertAsync("RequisitionDetail", d.ID, Body.ID, d.DynamicForm, actor)`; `SaveChangesAsync()`; commit.
- **GET (detail & list)** — setelah query, `LoadAsync(...)` batch untuk header + semua detail, set `DynamicForm` sebelum serialisasi.
- **Delete** — cukup andalkan `ON DELETE CASCADE` (`Requisition → RequisitionDynamicForm → RequisitionDynamicFormData`). Untuk hapus **satu detail**, hapus `RequisitionDynamicForm` yang `EntityType='RequisitionDetail' AND EntityID=d.ID` (cascade ke Data).
- **Validasi backend** — di `UpsertAsync`, jangan percaya frontend: jalankan ulang `required`/`requiredWhen`/`validation` dari `SchemaSnapshot`.

### 6.4 `postSubmitItems` (fase lanjut, opsional)

Port `resolveAutoIRStepQty` + loop `postSubmitItems` dari `auto_ir_service.go`: sesudah requisition tersimpan, baca `Data`, hitung qty dari `qtyFromStep`/`qtyFromField`, `AddAsync` `RequisitionDetail` baru. Idempoten (penanda di form header, mis. field `AutoItemsGenerated=true`).

---

## 7. Migrasi kolom hardcoded → dinamis (Expand → Backfill → Dual-write → Switch → Contract)

Jangan drop kolom langsung.

1. **EXPAND** — jalankan migration §3. Seed `RequisitionDynamicFormTemplate` (`IsPublished=true`) untuk tiap kategori yang kini pakai kolom khusus: Freon, Wire Rope, Electric Motor, Diameter.
2. **BACKFILL** — untuk tiap `RequisitionDetail` dengan kolom khusus non-NULL: buat `RequisitionDynamicForm` + isi `RequisitionDynamicFormData`.
   ```sql
   -- contoh untuk detail kategori Wire Rope
   INSERT INTO "RequisitionDynamicForm"
       ("RequisitionID","EntityType","EntityID","TemplateID","TemplateCode","TemplateVersion","SchemaSnapshot","Status")
   SELECT d."RequisitionID", 'RequisitionDetail', d."ID", t."ID", t."Code", t."Version", t."SchemaJson", 'Completed'
   FROM "RequisitionDetail" d
   JOIN "Requisition" r ON r."ID" = d."RequisitionID"
   JOIN "RequisitionDynamicFormTemplate" t ON t."Code" = 'WIRE_ROPE' AND t."IsPublished"
   WHERE d."WireRopeEndType" IS NOT NULL
   ON CONFLICT ("EntityType","EntityID") DO NOTHING;

   INSERT INTO "RequisitionDynamicFormData" ("FormID","StepKey","FieldKey","ValueType","ValueText")
   SELECT f."ID", 'identifikasi', 'EndType', 'text', d."WireRopeEndType"
   FROM "RequisitionDynamicForm" f
   JOIN "RequisitionDetail" d ON d."ID" = f."EntityID" AND f."EntityType"='RequisitionDetail'
   WHERE d."WireRopeEndType" IS NOT NULL
   ON CONFLICT ("FormID","FieldKey") DO NOTHING;
   -- ulangi per kolom: Brand, Model, Size, InnerDiameter(number), OuterDiameter(number),
   -- FreonSystem(text), FreonDamageReportRequired(bool), dst.
   ```
   Validasi: jumlah baris Data = jumlah kolom non-NULL sumber.
3. **DUAL-WRITE** — controller isi **keduanya** (kolom lama + tabel dinamis) ≥1 rilis. Rollback aman.
4. **SWITCH READ** — response & laporan baca dari tabel dinamis; frontend pindah ke wizard.
5. **CONTRACT** — setelah stabil & ada dump: `ALTER TABLE "RequisitionDetail" DROP COLUMN "FreonSystem", DROP COLUMN "WireRopeEndType", ...;` hapus properti terkait di `RequisitionDetail.cs` + `ApplyFreonEvaluationAsync` / `ApplyWireRopeEvaluationAsync` (aturannya pindah ke blok `validation` template atau service generik).

Kolom yang **tetap** kolom nyata (jangan dinamiskan): `ItemID`, `QtyRequest`, `QtyApproved*`, `RequisitionID`, `Remarks`, semua field status/approval — dipakai query inti, join, agregasi keuangan.

---

## 8. Alur frontend (dynamic wizard)

1. User pilih **Item** → frontend tahu `CategoryID` / `ItemType`.
2. `GET RequisitionDynamicFormTemplate?entityType=RequisitionDetail&categoryId=…&itemType=…` → dapat template + `SchemaJson`. Kosong → form datar standar / tanpa wizard.
3. Render **stepper**: satu langkah per `steps[]`; *Next* aktif bila field `required` di step valid; step di-skip bila `visibleWhen` false.
4. `prefill[]` diterapkan saat step dibuka.
5. Submit → susun `DynamicForm = { templateCode, templateVersion, values: { fieldKey: value, ... } }` di body `Requisition` / tiap `RequisitionDetail`.
6. Response mengembalikan `DynamicForm` yang dirakit ulang → dipakai halaman detail & review-approve (render read-only dengan `SchemaSnapshot` dari form, bukan template terbaru).

---

## 9. Checklist verifikasi

- [ ] Migration §3 jalan idempoten di staging (3 tabel + FK cascade + unique index).
- [ ] 3 `DbSet` + mapping `jsonb` + relasi cascade terdaftar di `BeesuiteDbContext`.
- [ ] `GET RequisitionDynamicFormTemplate` resolusi `ItemType → Category → Global` benar; hanya `IsActive && IsPublished`.
- [ ] PUT template yang sudah dipakai → dibuat `Version` baru, bukan overwrite.
- [ ] `RequisitionDynamicForm` menyimpan `SchemaSnapshot` saat dibuat; render detail lama pakai snapshot.
- [ ] `UpsertAsync` round-trip: decode→encode `values` identik, termasuk nilai bersarang (`ValueJson`) & `null`.
- [ ] Create/Update Requisition menulis Form+Data dalam **satu transaksi** dengan header+detail.
- [ ] GET detail & list meng-*overlay* `DynamicForm` (header + semua detail) via batch `LoadAsync`.
- [ ] Hapus Requisition → `RequisitionDynamicForm` & `...FormData` ikut terhapus (uji cascade DB).
- [ ] Hapus satu `RequisitionDetail` → form & data-nya ikut terhapus.
- [ ] Validasi `required`/`requiredWhen`/`validation` dievaluasi ulang di backend.
- [ ] Backfill: jumlah baris `...FormData` = jumlah kolom lama non-NULL.
- [ ] Dual-write aktif ≥1 rilis sebelum `DROP COLUMN`.
- [ ] Laporan agregasi numerik pakai `ValueNumber` (bukan cast dari teks).

---

## 10. Ringkas

1. Requisition dapat **3 tabel sendiri** (tidak *reuse* `JobFieldValue` Job):
   - **`RequisitionDynamicFormTemplate`** — blueprint wizard, *versioned*, di-scope per Category/Family/ItemType/global. Padanan `RepairEquipmentItems.StepConfigJson` + `JobFieldDefinition`.
   - **`RequisitionDynamicForm`** — 1 instance per `Requisition`/`RequisitionDetail`, menautkan ke template + **snapshot schema**, anchor untuk cascade.
   - **`RequisitionDynamicFormData`** — nilai per field (EAV, kolom nilai paralel + `ValueJson` lossless). Padanan `JobFieldValue`/`StepValues`.
2. FK tegas + `ON DELETE CASCADE` → hapus otomatis, tak perlu cascade manual.
3. Port adapter dari `job_field_value.go` ke `RequisitionDynamicFormService`; wire ke `RequisitionController` Create/Update/Get/Delete dalam transaksi.
4. Migrasi kolom `Freon*`/`WireRope*`/dimensi bertahap: **expand → backfill → dual-write → switch → contract**.
5. Frontend: `GET` template → render stepper → submit `DynamicForm { templateCode, templateVersion, values{} }`.
