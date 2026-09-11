-- ============================================================================
-- Add Fleet scoping to the V2 group-tier approval assignment (UserApprovalGroup),
-- and mirror the vessel's Fleet onto the Postgres vessel table so the resolver
-- can match against it (see UserController.ResolveScopeCandidatesV2).
--
-- Fleet is a plain text vessel-type category (e.g. "TANKER I", "BULK CARRIER",
-- "TOWING BARGE") -- shared vocabulary across all companies, not per-tenant --
-- sourced from SQL Server API.Ascend.Vessel.Fleet (no per-tenant FK needed).
--
-- UserApprovalGroup.Fleet is nullable: NULL = assignment applies to every
-- fleet (same wildcard convention as ItemGroupMapping.FamilyID == null).
--
-- "Ascend"."IC_InventoryUsers" is not live-synced from SQL Server by anything
-- in this repo -- Fleet is backfilled once from SQL Server data after this
-- migration runs (see backfill step run alongside this file), same as
-- ApprovalRuleVersion before it.
--
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

ALTER TABLE "Ascend"."IC_InventoryUsers" ADD COLUMN IF NOT EXISTS "Fleet" varchar NULL;
ALTER TABLE "UserApprovalGroup" ADD COLUMN IF NOT EXISTS "Fleet" varchar NULL;
