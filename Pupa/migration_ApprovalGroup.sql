-- ============================================================================
-- ApprovalGroup: master list of the 8 business Group names used by the
-- Approval Matrix (Approval Rule V2 prep). ItemGroupMapping and
-- UserApprovalGroup are wired to this table via a real GroupID foreign key
-- (GroupName text columns are kept as denormalized display copies).
-- Reference/staging only -- NOT wired into any V1 approval logic.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

CREATE TABLE IF NOT EXISTS "ApprovalGroup" (
    "ID"         serial PRIMARY KEY,
    "GroupName"  varchar(200) NOT NULL UNIQUE,
    "CreatedAt"  timestamptz  NULL DEFAULT now()
);

INSERT INTO "ApprovalGroup" ("GroupName")
SELECT DISTINCT "GroupName" FROM "ItemGroupMapping"
UNION
SELECT DISTINCT "GroupName" FROM "UserApprovalGroup"
ON CONFLICT ("GroupName") DO NOTHING;

-- ItemGroupMapping.GroupID -----------------------------------------------
ALTER TABLE "ItemGroupMapping" ADD COLUMN IF NOT EXISTS "GroupID" integer NULL;

UPDATE "ItemGroupMapping" igm
SET "GroupID" = ag."ID"
FROM "ApprovalGroup" ag
WHERE ag."GroupName" = igm."GroupName" AND igm."GroupID" IS NULL;

ALTER TABLE "ItemGroupMapping" ALTER COLUMN "GroupID" SET NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_ItemGroupMapping_ApprovalGroup'
    ) THEN
        ALTER TABLE "ItemGroupMapping"
            ADD CONSTRAINT "FK_ItemGroupMapping_ApprovalGroup"
            FOREIGN KEY ("GroupID") REFERENCES "ApprovalGroup"("ID");
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_ItemGroupMapping_GroupID" ON "ItemGroupMapping" ("GroupID");

-- UserApprovalGroup.GroupID -----------------------------------------------
ALTER TABLE "UserApprovalGroup" ADD COLUMN IF NOT EXISTS "GroupID" integer NULL;

UPDATE "UserApprovalGroup" uag
SET "GroupID" = ag."ID"
FROM "ApprovalGroup" ag
WHERE ag."GroupName" = uag."GroupName" AND uag."GroupID" IS NULL;

ALTER TABLE "UserApprovalGroup" ALTER COLUMN "GroupID" SET NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_UserApprovalGroup_ApprovalGroup'
    ) THEN
        ALTER TABLE "UserApprovalGroup"
            ADD CONSTRAINT "FK_UserApprovalGroup_ApprovalGroup"
            FOREIGN KEY ("GroupID") REFERENCES "ApprovalGroup"("ID");
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_UserApprovalGroup_GroupID" ON "UserApprovalGroup" ("GroupID");
