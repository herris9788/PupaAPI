-- ============================================================================
-- Add ConsumedBy + COA columns to the Usage document header.
--   ConsumedBy : the person/crew who actually consumed the items (mandatory in app)
--   COA        : Chart of Accounts (StockFamilyCOA.PurposeEx) charged for the usage
-- PostgreSQL. Run once against the Beesuite database.
-- ============================================================================

ALTER TABLE "Usage" ADD COLUMN IF NOT EXISTS "ConsumedBy" varchar(255) NULL;
ALTER TABLE "Usage" ADD COLUMN IF NOT EXISTS "COA"        varchar(255) NULL;
