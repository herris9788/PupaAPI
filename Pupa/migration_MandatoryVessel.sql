-- ============================================================================
-- MandatoryVessel / MandatoryVesselApprover: Approval Rule V2 prep.
--
-- For a vessel marked "wajib" (mandatory) in MandatoryVessel, approval levels
-- 1..MandatoryLevelCutoff must go through the specific named user(s) listed
-- in MandatoryVesselApprover for that (Vessel, CompanyDB, Level). Levels
-- beyond the cutoff -- and every level for vessels NOT listed here -- fall
-- through to the generic Group-based lookup already built
-- (UserApprovalGroup / ApprovalGroup).
--
-- Reference/staging tables only -- NOT wired into any V1 approval logic.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

CREATE TABLE IF NOT EXISTS "MandatoryVessel" (
    "ID"                    serial PRIMARY KEY,
    "VesselID"              integer      NOT NULL,
    "CompanyDB"             varchar(20)  NOT NULL,
    "MandatoryLevelCutoff"  smallint     NOT NULL,
    "CreatedAt"             timestamptz  NULL DEFAULT now(),
    UNIQUE ("VesselID", "CompanyDB")
);

CREATE TABLE IF NOT EXISTS "MandatoryVesselApprover" (
    "ID"         serial PRIMARY KEY,
    "VesselID"   integer      NOT NULL,
    "CompanyDB"  varchar(20)  NOT NULL,
    "Level"      smallint     NOT NULL,
    "Username"   varchar(255) NOT NULL,
    "CreatedAt"  timestamptz  NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS "IX_MandatoryVesselApprover_Vessel_Company_Level"
    ON "MandatoryVesselApprover" ("VesselID", "CompanyDB", "Level");
CREATE INDEX IF NOT EXISTS "IX_MandatoryVesselApprover_Username"
    ON "MandatoryVesselApprover" ("Username");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_MandatoryVesselApprover_MandatoryVessel'
    ) THEN
        ALTER TABLE "MandatoryVesselApprover"
            ADD CONSTRAINT "FK_MandatoryVesselApprover_MandatoryVessel"
            FOREIGN KEY ("VesselID", "CompanyDB")
            REFERENCES "MandatoryVessel" ("VesselID", "CompanyDB")
            ON DELETE CASCADE;
    END IF;
END $$;
