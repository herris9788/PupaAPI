-- ============================================================================
-- Per-item approval quantities on JobDetail (Job Request).
-- Mirrors RequisitionDetail: Job Request approval now works like Item Request,
-- where each approver on the document-level chain can adjust the approved
-- quantity per item. QtyApproved holds the current/running value; QtyApproved1..7
-- snapshot what each approval level signed off on. UOMLevel records which UOM
-- tier the quantity is expressed in.
-- PostgreSQL. Run once against the Beesuite database. Schema is managed
-- manually (no EF migrations).
-- ============================================================================

ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyRequest"  numeric(18,4) NULL DEFAULT 0;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved" numeric(18,4) NULL DEFAULT 0;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved1" numeric(18,4) NULL;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved2" numeric(18,4) NULL;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved3" numeric(18,4) NULL;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved4" numeric(18,4) NULL;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved5" numeric(18,4) NULL;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved6" numeric(18,4) NULL;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "QtyApproved7" numeric(18,4) NULL;
ALTER TABLE public."JobDetail" ADD COLUMN IF NOT EXISTS "UOMLevel" integer NULL;
