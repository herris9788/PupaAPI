-- ============================================================================
-- Add a "Flag" column to vessels (InventoryUser).
-- Free-form, hashtag-style tags stored as a ';'-separated list
-- (e.g. 'paint-feature;bwms'), used to mark which vessels are expected to have
-- certain features. PostgreSQL. Run once against the Beesuite database.
-- ============================================================================

ALTER TABLE "Ascend"."IC_InventoryUsers"
    ADD COLUMN IF NOT EXISTS "Flag" text NULL;
