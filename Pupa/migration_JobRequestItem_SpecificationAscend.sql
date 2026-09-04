-- ============================================================================
-- JobRequestItem: tambah kolom SpecificationAscend (text).
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

ALTER TABLE "JobRequestItem" ADD COLUMN IF NOT EXISTS "SpecificationAscend" text NULL;
