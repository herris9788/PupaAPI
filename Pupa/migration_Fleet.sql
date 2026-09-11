-- ============================================================================
-- Add Fleet scoping to the V2 group-tier approval assignment (UserApprovalGroup),
-- and mirror the vessel's Fleet onto the Postgres vessel table so the resolver
-- can match against it (see UserController.ResolveScopeCandidatesV2).
--
-- Fleet is a plain text COARSE vessel-type bucket (e.g. "TANKER", "BULK
-- CARRIER", "TOWING", "TUG BOAT") -- shared vocabulary across all companies,
-- not per-tenant. Source column is SQL Server API.Ascend.Vessel.Fleet, but
-- that column actually stores the finer-grained "Inventory User Group" style
-- label (e.g. "TANKER I", "TUG BOAT BELAWAN") -- Fleet here is that label
-- collapsed to its coarse bucket (roman-numeral tanker variants + SMALL
-- TANKER -> TANKER; the 4 city TUG BOAT variants -> TUG BOAT; TOWING BARGE ->
-- TOWING; BULK CARRIER / CEMENT CARRIER unchanged), confirmed with the user.
--
-- UserApprovalGroup.Fleet is nullable: NULL = assignment applies to every
-- fleet (same wildcard convention as ItemGroupMapping.FamilyID == null).
--
-- "Ascend"."IC_InventoryUsers" is not live-synced from SQL Server by anything
-- in this repo -- Fleet is backfilled once from SQL Server data (then
-- collapsed to its coarse bucket) after this migration runs, same as
-- ApprovalRuleVersion before it.
--
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

ALTER TABLE "Ascend"."IC_InventoryUsers" ADD COLUMN IF NOT EXISTS "Fleet" varchar NULL;
ALTER TABLE "UserApprovalGroup" ADD COLUMN IF NOT EXISTS "Fleet" varchar NULL;
