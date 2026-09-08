-- ============================================================================
-- JobRequestItem: tambah kolom JobLocation, DetailJobType, AeNumber,
-- ReasonRequisition (JSON string, diparsing di client) dan ItemCategory,
-- ItemFamily (untuk FABRICATION).
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

ALTER TABLE "JobRequestItem" ADD COLUMN IF NOT EXISTS "JobLocation" text NULL;
ALTER TABLE "JobRequestItem" ADD COLUMN IF NOT EXISTS "DetailJobType" text NULL;
ALTER TABLE "JobRequestItem" ADD COLUMN IF NOT EXISTS "AeNumber" text NULL;
ALTER TABLE "JobRequestItem" ADD COLUMN IF NOT EXISTS "ReasonRequisition" text NULL;
ALTER TABLE "JobRequestItem" ADD COLUMN IF NOT EXISTS "ItemCategory" text NULL;
ALTER TABLE "JobRequestItem" ADD COLUMN IF NOT EXISTS "ItemFamily" text NULL;
