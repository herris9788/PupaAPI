-- Job Request draft cart — mirrors CartItem's role for Item Request.
-- One row per not-yet-submitted request card; deleted once the draft is
-- actually submitted. See BusinessObjects/Beesuite/JobRequestCartItem.cs.

CREATE TABLE IF NOT EXISTS "JobRequestCartItem" (
    "ID" SERIAL PRIMARY KEY,
    "UserName" character varying NOT NULL,
    "VesselID" integer NOT NULL,
    "VesselName" character varying,
    "CompanyDB" character varying,
    "CategoryName" character varying,
    "SubCategoryName" character varying,
    "FormType" character varying,
    "SortOrder" integer NOT NULL DEFAULT 0,
    "OrderPurpose" text,
    "CalibrationLocation" character varying,
    "WizardData" jsonb,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "CreatedBy" character varying,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "UpdatedBy" character varying
);

CREATE INDEX IF NOT EXISTS "IX_JobRequestCartItem_UserVessel"
    ON "JobRequestCartItem" ("UserName", "VesselID", "IsActive");
