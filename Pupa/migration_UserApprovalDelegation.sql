-- ============================================================================
-- Approval delegation (user is absent -> delegate to another user)
-- PostgreSQL. Run once against the Beesuite database.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

-- 1) User-to-user delegation, time-bound (absence window).
CREATE TABLE IF NOT EXISTS "UserApprovalDelegation" (
    "ID"          serial       PRIMARY KEY,
    "FromUserID"  integer      NOT NULL,   -- the original approver (away)
    "ToUserID"    integer      NOT NULL,   -- the delegate (acts on their behalf)
    "StartDate"   timestamp    NULL,       -- absence window start (NULL = now)
    "EndDate"     timestamp    NULL,       -- absence window end   (NULL = open-ended)
    "IsActive"    boolean      NULL DEFAULT TRUE,
    "Note"        varchar(255) NULL,
    "CreatedAt"   timestamp    NULL DEFAULT (now() AT TIME ZONE 'utc'),
    "CreatedBy"   varchar(100) NULL,
    CONSTRAINT "FK_UserApprovalDelegation_FromUser"
        FOREIGN KEY ("FromUserID") REFERENCES "User" ("ID"),
    CONSTRAINT "FK_UserApprovalDelegation_ToUser"
        FOREIGN KEY ("ToUserID")   REFERENCES "User" ("ID")
);

CREATE INDEX IF NOT EXISTS "IX_UserApprovalDelegation_ToUserID"
    ON "UserApprovalDelegation" ("ToUserID");
CREATE INDEX IF NOT EXISTS "IX_UserApprovalDelegation_FromUserID"
    ON "UserApprovalDelegation" ("FromUserID");

-- 2) Audit columns on Requisition.
--    When a delegate approves, ApprovedByN keeps the ORIGINAL approver and
--    ApprovedByNActualBy records the delegate who actually performed it.
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "ApprovedBy1ActualBy" varchar(500) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "ApprovedBy2ActualBy" varchar(500) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "ApprovedBy3ActualBy" varchar(500) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "ApprovedBy4ActualBy" varchar(500) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "ApprovedBy5ActualBy" varchar(500) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "ApprovedBy6ActualBy" varchar(500) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "ApprovedBy7ActualBy" varchar(500) NULL;
