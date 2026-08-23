-- ============================================================================
-- UserApprovalScope2 — approval scope, konsep baru berbasis
-- Group/Category/Family/Vessel/VesselGroup/Department/SubDepartment/DB/Level.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
--
-- Beda dari UserApprovalScope lama: semua kolom scope NULLable dengan
-- semantik konsisten NULL = wildcard (berlaku untuk semua nilai pada dimensi
-- itu). Tabel lama mewajibkan CompanyDB/VesselGroupID NOT NULL dan pakai -1
-- sebagai sentinel "tidak spesifik" untuk StockCategoryID/StockFamilyID.
-- ============================================================================

CREATE TABLE IF NOT EXISTS "UserApprovalScope2" (
    "ID"              serial                   PRIMARY KEY,
    "UserID"          integer                  NOT NULL,  -- approver untuk scope ini
    "CompanyDB"       varchar(20)              NULL,      -- NULL = semua DB
    "Group"           varchar(50)              NULL,      -- NULL = semua Group
    "StockCategoryID" integer                  NULL,      -- NULL = semua Category
    "StockFamilyID"   integer                  NULL,      -- NULL = semua Family
    "VesselID"        integer                  NULL,      -- NULL = semua Vessel dalam VesselGroup
    "VesselGroupID"   integer                  NULL,      -- NULL = semua VesselGroup
    "Department"      varchar(10)              NULL,      -- NULL = semua Department
    "SubDepartment"   varchar                  NULL,      -- NULL = semua SubDepartment
    "Level"           smallint                 NULL,      -- NULL = semua Level; kalau diisi 1..7
    "IsActive"        boolean                  NULL DEFAULT TRUE,
    "CreatedAt"       timestamptz              NOT NULL DEFAULT now(),
    "UpdatedAt"       timestamptz              NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS "idx_uas2_user_id"           ON "UserApprovalScope2" ("UserID");
CREATE INDEX IF NOT EXISTS "idx_uas2_vessel_group_id"   ON "UserApprovalScope2" ("VesselGroupID");
CREATE INDEX IF NOT EXISTS "idx_uas2_vessel_id"         ON "UserApprovalScope2" ("VesselID");
CREATE INDEX IF NOT EXISTS "idx_uas2_level"              ON "UserApprovalScope2" ("Level");
