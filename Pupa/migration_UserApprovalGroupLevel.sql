-- ============================================================================
-- UserApprovalGroup: add Level (group-relative approval level, 1-based).
-- The resolver offsets this by the vessel's MandatoryLevelCutoff to get the
-- real approval-chain position (see UserController.ResolveScopeCandidatesV2).
-- Existing rows (seeded from V1 data, no level distinction) default to 1.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

ALTER TABLE "UserApprovalGroup" ADD COLUMN IF NOT EXISTS "Level" smallint NOT NULL DEFAULT 1;

CREATE INDEX IF NOT EXISTS "IX_UserApprovalGroup_GroupName_Level"
    ON "UserApprovalGroup" ("GroupName", "Level");
