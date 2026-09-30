-- ============================================================================
-- JobDetail.FormDataJson — menyimpan jawaban form mentah (JSON) yang dulu
-- cuma diratakan jadi teks di JobDetail.Remarks lalu dibuang strukturnya.
-- Dipakai Review & Approve supaya approver bisa buka ulang form terstruktur
-- yang sama (bukan cuma baca teks Remarks) — lihat CalibrationForm reuse di
-- ReviewApproveOrderPageWeb.dart. Baru dipakai kategori Calibration; kategori
-- Job Request lain masih NULL sampai masing-masing di-wire.
--
-- Additive: NULL untuk semua baris lama, tidak mengubah behaviour yang sudah
-- ada. Idempoten. PostgreSQL. Run: staging dulu, verifikasi, baru production.
-- ============================================================================

BEGIN;

ALTER TABLE "JobDetail" ADD COLUMN IF NOT EXISTS "FormDataJson" jsonb NULL;

COMMIT;

-- Verifikasi:
-- SELECT "JobID", "Category", "FormDataJson" FROM "JobDetail" WHERE "FormDataJson" IS NOT NULL ORDER BY "JobID" DESC LIMIT 20;
