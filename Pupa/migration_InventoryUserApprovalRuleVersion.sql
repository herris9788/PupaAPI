-- ============================================================================
-- Add an "ApprovalRuleVersion" column to vessels (InventoryUser).
-- Picks which approval-scope table drives that vessel's approval routing:
-- NULL or 1 = UserApprovalScope (old Company/VesselGroup/Vessel/Category/Family
-- cascade), 2 = UserApprovalScope2 (Specificity-based, every dimension
-- independently NULLable/wildcard). NULL default keeps every existing vessel
-- on the old behavior — no data migration needed.
-- PostgreSQL. Run once against the Beesuite database.
-- ============================================================================

ALTER TABLE "Ascend"."IC_InventoryUsers"
    ADD COLUMN IF NOT EXISTS "ApprovalRuleVersion" smallint NULL;
