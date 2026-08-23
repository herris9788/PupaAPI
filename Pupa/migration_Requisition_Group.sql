-- ============================================================================
-- Requisition.Group — snapshot of the Group this requisition was routed under
-- (Item Request V2 combined submission). PostgreSQL. Run once against
-- beesuite_staging, then beesuite. Schema is managed manually (no EF
-- migrations), matching the existing convention in this repo.
-- ============================================================================
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "Group" varchar(50) NULL;
CREATE INDEX IF NOT EXISTS "idx_requisition_group" ON "Requisition" ("Group");
