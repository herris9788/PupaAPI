-- ============================================================================
-- Item usage documents (Usage header + UsageDetail lines). One usage records
-- the consumption of one or more items on board and can go through an approval
-- flow. PostgreSQL. Run once against the Beesuite database.
-- Schema is managed manually (no EF migrations).
--
-- NOTE: This is a standalone log. Saving a usage does NOT change the ROB
-- (remaining on board) stock. Item / vessel details are snapshotted so each
-- row stays self-contained for reporting.
-- ============================================================================

-- 1) Header -------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Usage" (
    "ID"                serial        PRIMARY KEY,
    "DocumentNumber"    varchar(100)  NULL,
    "DB"                varchar(100)  NULL,        -- source database / company
    "InventoryUserID"   integer       NULL,        -- vessel (InventoryUser) PK
    "InventoryUserCode" varchar(100)  NULL,        -- vessel code
    "VesselName"        varchar(255)  NULL,        -- snapshot of vessel name
    "UsageDate"         timestamp     NULL,        -- when the items were used
    "UsedBy"            varchar(255)  NULL,        -- crew / person who used them
    "Purpose"           varchar(500)  NULL,        -- why they were used
    "Remarks"           varchar(1000) NULL,
    "Status"            varchar(100)  NULL DEFAULT 'Pending Approval',
    "Approved"          boolean       NULL DEFAULT FALSE,
    "ApprovalMaxLevel"  integer       NULL,
    "RejectedBy"        varchar(500)  NULL,
    "RejectedTime"      timestamp     NULL,
    "CreatedBy"         varchar(100)  NULL,
    "CreatedAt"         timestamp     NULL DEFAULT (now() AT TIME ZONE 'utc'),
    "UpdatedAt"         timestamp     NULL DEFAULT (now() AT TIME ZONE 'utc'),
    -- Approval chain (mirrors Requisition) -----------------------------------
    "ApprovedBy1"        varchar(500) NULL,
    "ApprovedBy2"        varchar(500) NULL,
    "ApprovedBy3"        varchar(500) NULL,
    "ApprovedBy4"        varchar(500) NULL,
    "ApprovedBy5"        varchar(500) NULL,
    "ApprovedBy6"        varchar(500) NULL,
    "ApprovedBy7"        varchar(500) NULL,
    "ApprovedBy1At"      timestamp    NULL,
    "ApprovedBy2At"      timestamp    NULL,
    "ApprovedBy3At"      timestamp    NULL,
    "ApprovedBy4At"      timestamp    NULL,
    "ApprovedBy5At"      timestamp    NULL,
    "ApprovedBy6At"      timestamp    NULL,
    "ApprovedBy7At"      timestamp    NULL,
    "ApprovedBy1ActualBy" varchar(500) NULL,
    "ApprovedBy2ActualBy" varchar(500) NULL,
    "ApprovedBy3ActualBy" varchar(500) NULL,
    "ApprovedBy4ActualBy" varchar(500) NULL,
    "ApprovedBy5ActualBy" varchar(500) NULL,
    "ApprovedBy6ActualBy" varchar(500) NULL,
    "ApprovedBy7ActualBy" varchar(500) NULL
);

CREATE INDEX IF NOT EXISTS "IX_Usage_InventoryUserID"
    ON "Usage" ("InventoryUserID");
CREATE INDEX IF NOT EXISTS "IX_Usage_UsageDate"
    ON "Usage" ("UsageDate");
CREATE INDEX IF NOT EXISTS "IX_Usage_Status"
    ON "Usage" ("Status");

-- 2) Detail -------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "UsageDetail" (
    "ID"             serial        PRIMARY KEY,
    "UsageID"        integer       NULL,           -- parent Usage
    "ItemID"         integer       NULL,
    "ItemCode"       varchar(100)  NULL,
    "ItemName"       varchar(500)  NULL,           -- snapshot of item name
    "Specification"  varchar(255)  NULL,
    "PartNoEx"       varchar(255)  NULL,
    "Category"       varchar(255)  NULL,
    "Family"         varchar(255)  NULL,
    "VesselPost"     varchar(100)  NULL,           -- Deck / Engine
    "UOM"            varchar(50)   NULL,           -- unit of measure
    "Qty"            numeric       NULL,           -- quantity consumed
    "Remarks"        varchar(1000) NULL,
    CONSTRAINT "FK_UsageDetail_Usage"
        FOREIGN KEY ("UsageID") REFERENCES "Usage" ("ID")
);

CREATE INDEX IF NOT EXISTS "IX_UsageDetail_UsageID"
    ON "UsageDetail" ("UsageID");
CREATE INDEX IF NOT EXISTS "IX_UsageDetail_ItemID"
    ON "UsageDetail" ("ItemID");
