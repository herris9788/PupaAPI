# Dynamic Requisition Wizard — Perubahan Database

> PostgreSQL. Dikelola manual (`migration_*.sql`). Semua langkah idempoten.

## 1. Tabel baru

### `RequisitionDynamicFormTemplate` — blueprint wizard (versioned)
| Kolom | Tipe | Ket |
|---|---|---|
| `ID` | integer identity PK | |
| `Code` | varchar(100) NOT NULL | mis. `FREON`, `WIRE_ROPE`, `DEFAULT` |
| `Name` | varchar(255) NOT NULL | |
| `Version` | integer NOT NULL DEFAULT 1 | |
| `Scope` | varchar(30) NOT NULL DEFAULT `'Category'` | `Category` \| `Family` \| `ItemType` \| `Global` |
| `CategoryID` | integer NULL | FK `StockFamily.ID` |
| `ItemType` | integer NULL | |
| `EntityType` | varchar(30) NOT NULL DEFAULT `'RequisitionDetail'` | `Requisition` \| `RequisitionDetail` |
| `IsActive` | boolean NOT NULL DEFAULT true | |
| `IsPublished` | boolean NOT NULL DEFAULT false | |
| `SchemaJson` | jsonb NOT NULL DEFAULT `'{}'` | definisi step→field, kondisi, validasi |
| `CreatedAt` / `UpdatedAt` | timestamptz NOT NULL DEFAULT now() | |
| `CreatedBy` | varchar(100) NULL | |

- UNIQUE `(Code, Version)`
- INDEX `(EntityType, Scope, CategoryID, ItemType, IsActive, IsPublished)`

### `RequisitionDynamicForm` — instance per Requisition / RequisitionDetail
| Kolom | Tipe | Ket |
|---|---|---|
| `ID` | integer identity PK | |
| `RequisitionID` | integer NOT NULL | FK `Requisition.ID` **ON DELETE CASCADE** |
| `EntityType` | varchar(30) NOT NULL | `Requisition` \| `RequisitionDetail` |
| `EntityID` | integer NOT NULL | `Requisition.ID` atau `RequisitionDetail.ID` |
| `TemplateID` | integer NOT NULL | FK `RequisitionDynamicFormTemplate.ID` |
| `TemplateCode` | varchar(100) NOT NULL | |
| `TemplateVersion` | integer NOT NULL | |
| `SchemaSnapshot` | jsonb NOT NULL | salinan `SchemaJson` saat form dibuat |
| `Status` | varchar(20) NOT NULL DEFAULT `'Draft'` | `Draft` \| `Completed` |
| `CreatedAt` / `UpdatedAt` | timestamptz NOT NULL DEFAULT now() | |
| `CreatedBy` | varchar(100) NULL | |

- UNIQUE `(EntityType, EntityID)`
- INDEX `(RequisitionID)`

### `RequisitionDynamicFormData` — nilai per field (EAV)
| Kolom | Tipe | Ket |
|---|---|---|
| `ID` | integer identity PK | |
| `FormID` | integer NOT NULL | FK `RequisitionDynamicForm.ID` **ON DELETE CASCADE** |
| `StepKey` | varchar(100) NULL | |
| `FieldKey` | varchar(100) NOT NULL | |
| `ValueType` | varchar(20) NOT NULL DEFAULT `'text'` | `text`\|`number`\|`date`\|`bool`\|`json`\|`null` |
| `ValueText` | text NULL | |
| `ValueNumber` | numeric NULL | |
| `ValueDate` | date NULL | |
| `ValueBool` | boolean NULL | |
| `ValueJson` | jsonb NULL | fallback nilai bersarang |
| `CreatedAt` / `UpdatedAt` | timestamptz NOT NULL DEFAULT now() | |
| `CreatedBy` / `UpdatedBy` | varchar(100) NULL | |

- UNIQUE `(FormID, FieldKey)`
- INDEX `(FormID)`, INDEX `(FieldKey, ValueNumber)`

### `RequisitionDynamicFormDataHistory` — opsional (audit per-field)
`ID` PK · `FormID` int · `FieldKey` varchar(100) · `OldValue` text · `NewValue` text · `ChangedBy` varchar(100) · `ChangedAt` timestamptz DEFAULT now()
- INDEX `(FormID, FieldKey, ChangedAt)`

## 2. Tabel yang TIDAK diubah (fase awal)

`Requisition`, `RequisitionDetail` — **tidak ada kolom baru**. `DynamicForm` di API adalah properti `[NotMapped]` (transient), diisi/dibaca dari 3 tabel di atas.

## 3. Perubahan pada tabel lama (fase CONTRACT — paling akhir, setelah dual-write stabil)

`DROP COLUMN` di `RequisitionDetail` setelah nilainya dipindah ke `RequisitionDynamicFormData`:
`Brand`, `Model`, `Size`, `Edition`, `InnerDiameter`, `OuterDiameter`, `Length`,
`FreonSystem`, `FreonEvaluationScenario`, `FreonDamageReportRequired`, `FreonIntervalDays`, `FreonLastRequestDate`,
`WireRopeEndType`, `WireRopeRollQty`, `WireRopeEyeLengthM`, `WireRopeLeftEyeLengthM`, `WireRopeRightEyeLengthM`,
`PlacementArea`, `ReplacementReason`, `PurposeOfRequest` *(review per kolom sebelum drop)*.

Di `Requisition` (kandidat, review dulu):
`Machine`, `MotorType`, `IsOriginalElectricMotor`, `NumberOfRepair`, `RepairKitType`, `RunningHours`.

**Tetap kolom nyata (jangan disentuh):** `ItemID`, `QtyRequest`, `QtyApproved*`, `RequisitionID`, `Remarks`, semua kolom status/approval.

## 4. Urutan migrasi

1. **EXPAND** — `CREATE TABLE` 4 tabel baru + seed `RequisitionDynamicFormTemplate`.
2. **BACKFILL** — `INSERT` `RequisitionDynamicForm` + `RequisitionDynamicFormData` dari kolom lama non-NULL.
3. **DUAL-WRITE** — app tulis kolom lama + tabel baru (≥1 rilis).
4. **SWITCH READ** — app baca dari tabel baru.
5. **CONTRACT** — `DROP COLUMN` kolom lama (§3).

## 5. Cascade / integritas

- Hapus `Requisition` → `RequisitionDynamicForm` → `RequisitionDynamicFormData` terhapus otomatis (FK cascade).
- Hapus 1 `RequisitionDetail` → app hapus `RequisitionDynamicForm` where `EntityType='RequisitionDetail' AND EntityID=?` (cascade ke Data).
- `RequisitionDynamicFormTemplate` tidak pernah di-*hard delete* bila sudah direferensikan `RequisitionDynamicForm`; nonaktifkan via `IsActive=false`.
