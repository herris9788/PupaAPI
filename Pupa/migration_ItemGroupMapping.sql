-- ============================================================================
-- ItemGroupMapping: mapping COA -> Group barang (Approval Rule V2 prep).
-- Reference/staging table only -- NOT wired into any V1 approval logic.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

CREATE TABLE IF NOT EXISTS "ItemGroupMapping" (
    "ID"               serial PRIMARY KEY,
    "COACode"          varchar(20)  NOT NULL,
    "GroupName"        varchar(200) NOT NULL,
    "StockCategoryID"  integer      NOT NULL,
    "CategoryName"     varchar(100) NOT NULL,
    "FamilyID"         integer      NOT NULL,
    "FamilyCode"       varchar(30)  NOT NULL,
    "FamilyName"       varchar(200) NOT NULL,
    "Source"           varchar(30)  NOT NULL DEFAULT 'Approval Matrix',
    "CreatedAt"        timestamptz  NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS "IX_ItemGroupMapping_COACode" ON "ItemGroupMapping" ("COACode");
CREATE INDEX IF NOT EXISTS "IX_ItemGroupMapping_GroupName" ON "ItemGroupMapping" ("GroupName");
CREATE INDEX IF NOT EXISTS "IX_ItemGroupMapping_StockCategoryID_FamilyID" ON "ItemGroupMapping" ("StockCategoryID", "FamilyID");
