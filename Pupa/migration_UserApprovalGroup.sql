-- ============================================================================
-- UserApprovalGroup: Username -> Group barang (Approval Rule V2 prep).
-- Derived from each user's UserApprovalScope rows resolved through
-- ItemGroupMapping. Reference/staging table only -- NOT wired into any V1
-- approval logic.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

CREATE TABLE IF NOT EXISTS "UserApprovalGroup" (
    "ID"         serial PRIMARY KEY,
    "Username"   varchar(255) NOT NULL,
    "GroupName"  varchar(200) NOT NULL,
    "CreatedAt"  timestamptz  NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS "IX_UserApprovalGroup_Username" ON "UserApprovalGroup" ("Username");
CREATE INDEX IF NOT EXISTS "IX_UserApprovalGroup_GroupName" ON "UserApprovalGroup" ("GroupName");
