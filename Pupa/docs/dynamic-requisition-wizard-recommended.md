# Dynamic Requisition Wizard — Struktur DB

> Konsep. PostgreSQL + EF Core. 3 tabel: **Template → Config (per ItemCode) → Data**.

```
RequisitionFormTemplate      blueprint reusable (pustaka schema, versioned)
        │
RequisitionFormConfig        per ItemCode — pakai template ATAU schema sendiri
        │
RequisitionFormData          jawaban per requisition + snapshot config mentah
```

---

## 1. `RequisitionFormTemplate` — blueprint reusable

| Kolom | Tipe | Ket |
|---|---|---|
| `ID` | int identity PK | |
| `Code` | varchar(100) NOT NULL | `FREON`, `WIRE_ROPE`, `DIMENSI_UMUM`, … |
| `Name` | varchar(255) NOT NULL | |
| `Version` | int NOT NULL DEFAULT 1 | |
| `SchemaJson` | jsonb NOT NULL DEFAULT `'{}'` | steps → fields, kondisi, validasi, prefill |
| `IsActive` | boolean NOT NULL DEFAULT true | |
| `IsPublished` | boolean NOT NULL DEFAULT false | published = tidak boleh diubah lagi, edit → `Version` baru |
| `CreatedAt` / `UpdatedAt` | timestamptz NOT NULL DEFAULT now() | |
| `CreatedBy` | varchar(100) NULL | |

- UNIQUE `(Code, Version)`

## 2. `RequisitionFormConfig` — per ItemCode

| Kolom | Tipe | Ket |
|---|---|---|
| `ID` | int identity PK | |
| `ItemCode` | varchar(100) NOT NULL | relasi ke item master |
| `EntityType` | varchar(30) NOT NULL DEFAULT `'RequisitionDetail'` | `Requisition` \| `RequisitionDetail` |
| `Mode` | varchar(20) NOT NULL | `Template` \| `Custom` |
| `TemplateID` | int NULL | FK `RequisitionFormTemplate.ID` — diisi bila `Mode='Template'` |
| `SchemaJson` | jsonb NULL | schema sendiri — diisi bila `Mode='Custom'`; bila `Mode='Template'` boleh diisi sebagai *override* parsial atas template |
| `IsActive` | boolean NOT NULL DEFAULT true | |
| `CreatedAt` / `UpdatedAt` | timestamptz NOT NULL DEFAULT now() | |
| `CreatedBy` | varchar(100) NULL | |

- UNIQUE `(ItemCode, EntityType)` *(bila butuh histori config, tambah `Version` & longgarkan unique)*
- `CHECK (Mode='Template' AND "TemplateID" IS NOT NULL) OR (Mode='Custom' AND "SchemaJson" IS NOT NULL)`
- **Effective schema** (dihitung app): `Mode='Custom'` → `SchemaJson`; `Mode='Template'` → `Template.SchemaJson` di-*merge* dengan `SchemaJson` (override) bila ada.

## 3. `RequisitionFormData` — jawaban + snapshot config

| Kolom | Tipe | Ket |
|---|---|---|
| `ID` | int identity PK | |
| `RequisitionID` | int **NOT NULL** | FK `Requisition.ID` **ON DELETE CASCADE** |
| `RequisitionDetailID` | int **NULL** | FK `RequisitionDetail.ID` **ON DELETE CASCADE**; NULL = data milik header |
| `ItemCode` | varchar(100) NULL | snapshot |
| `ConfigID` | int NULL | FK `RequisitionFormConfig.ID` — referensi (boleh basi kalau config berubah) |
| `TemplateID` | int NULL | FK `RequisitionFormTemplate.ID` — referensi |
| `SchemaSnapshot` | jsonb NOT NULL | **config mentah (effective schema) saat submit** — sumber render/validasi selamanya untuk data ini |
| `Values` | jsonb NOT NULL DEFAULT `'{}'` | `{ "FieldKey": <nilai>, ... }` |
| `Status` | varchar(20) NOT NULL DEFAULT `'Draft'` | `Draft` \| `Completed` |
| `CreatedAt` / `UpdatedAt` | timestamptz NOT NULL DEFAULT now() | |
| `CreatedBy` / `UpdatedBy` | varchar(100) NULL | |

- `CHECK (("RequisitionDetailID" IS NULL) OR ("RequisitionID" IS NOT NULL))`
- UNIQUE `(RequisitionID, RequisitionDetailID)`
- INDEX `(RequisitionID)`, `(RequisitionDetailID)`, `(ItemCode)`
- `ConfigID`/`TemplateID` pakai `ON DELETE SET NULL` — snapshot tetap utuh walau config/template dihapus.

---

## 4. Tabel lama — TIDAK DISENTUH

- `Requisition` & `RequisitionDetail` tidak ditambah / diubah / dihapus kolomnya. Semua kolom lama (termasuk `Brand`, `Model`, `Freon*`, `WireRope*`, `Machine`, `MotorType`, dst) dibiarkan apa adanya — dipakai requisition lama.
- Integrasi dynamic form lewat properti `[NotMapped] DynamicForm` (transient, tidak memetakan kolom).

## 4a. Aturan wajib — jangan ganggu yang berjalan

- **Tidak ada perubahan pada perilaku Requisition yang sekarang.** Endpoint, payload, response, alur approval, dan pembuatan requisition existing harus tetap identik bila field dynamic form tidak dikirim.
- Kolom `Requisition` / `RequisitionDetail` tidak ditambah, diubah, atau dihapus.
- `DynamicForm` bersifat **opsional & aditif**: null/absen → requisition dibuat & dibaca persis seperti sebelum fitur ini ada.
- Logika existing (`ApplyFreonEvaluationAsync`, `ApplyWireRopeEvaluationAsync`, dll) tidak diubah.
- Semua tulis dynamic form masuk transaksi yang sama; gagal validasi dynamic form → rollback, jangan biarkan requisition tersimpan separuh.
- Jalankan build + test yang ada, pastikan hijau.

## 5. Rollout (tanpa backfill)

1. **EXPAND** — `CREATE TABLE` (§1–3) + seed template + config per ItemCode.
2. **GO-LIVE** — requisition baru pakai dynamic form. Requisition lama dibiarkan apa adanya, tetap render dari kolom hardcode-nya. Tidak ada backfill, tidak ada dual-write, tidak ada `DROP COLUMN`.

## 6. Cascade

- Hapus `Requisition`/`RequisitionDetail` → `RequisitionFormData` ikut (FK cascade).
- Hapus `RequisitionFormConfig`/`Template` → `RequisitionFormData.ConfigID`/`TemplateID` jadi NULL, `SchemaSnapshot` tetap.
- Template/Config tidak di-hard-delete bila masih aktif; nonaktifkan `IsActive=false`.
