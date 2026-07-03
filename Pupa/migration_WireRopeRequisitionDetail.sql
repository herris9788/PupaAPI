-- ============================================================================
-- Wire Rope (T31.001) requisition fields on RequisitionDetail.
-- Mirrors the "Wire Rope Requisition Process Flow" mandatory Define Item
-- inputs: Length is split into Roll + Meter (Meter reuses the existing
-- "Length" column), and End Type carries up to two optional eye lengths.
-- PostgreSQL. Run once against the Beesuite database. Schema is managed
-- manually (no EF migrations).
-- ============================================================================

ALTER TABLE "RequisitionDetail" ADD COLUMN IF NOT EXISTS "WireRopeRollQty" numeric(10,4) NULL;
ALTER TABLE "RequisitionDetail" ADD COLUMN IF NOT EXISTS "WireRopeEndType" character varying NULL;

-- Optional eye length inputs (meters). Left blank when the requester wants
-- the vendor default size — no default-size table exists yet, see the
-- TODO(wire-rope) notes in WireRopePolicy.cs / RequisitionController.cs.
ALTER TABLE "RequisitionDetail" ADD COLUMN IF NOT EXISTS "WireRopeEyeLengthM" numeric(10,4) NULL;
ALTER TABLE "RequisitionDetail" ADD COLUMN IF NOT EXISTS "WireRopeLeftEyeLengthM" numeric(10,4) NULL;
ALTER TABLE "RequisitionDetail" ADD COLUMN IF NOT EXISTS "WireRopeRightEyeLengthM" numeric(10,4) NULL;
