-- ============================================================================
-- Registers the "Mandatory Vessel Approval" admin page in the DB-driven
-- Menu/LaunchPoint permission system (kMenuCatalog in Sidenavbar.dart alone
-- is not enough -- a menu only actually renders once it has a Menu row AND
-- a LaunchPoint grant per user; see Sidenavbar.dart's _loadGrantedMenus).
-- Mirrors BS_APPROVAL_RULES_V2's setup exactly, granted to the same
-- BS_ROLE_SYSADMIN audience.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- Schema is managed manually (no EF migrations).
-- ============================================================================

INSERT INTO "Menu" (
    "MenuCode", "MenuName", "Icon", "Route", "Category", "Description",
    "SortOrder", "IsActive", "IsLogistic", "IsComingSoon",
    "AllowWeb", "AllowMobile", "HideAppBar", "AppBarShowLogo",
    "ShowBottomNav", "BackButtonMinDepth"
)
SELECT
    'BS_MANDATORY_VESSEL', 'Mandatory Vessel Approval', '', '/mandatory-vessel-approval', 'MANAGEMENT', '',
    14, true, false, false,
    true, false, false, false,
    false, 1
WHERE NOT EXISTS (SELECT 1 FROM "Menu" WHERE "MenuCode" = 'BS_MANDATORY_VESSEL');

-- Template grant (BS_ROLE_SYSADMIN), so any NEW SYSADMIN user also gets it.
INSERT INTO "LaunchPointTemplate" ("TemplateName", "MenuID", "Name", "Icon", "SortOrder", "IsActive", "IsLogistic")
SELECT 'BS_ROLE_SYSADMIN', m."ID", m."MenuName", '', 14, true, false
FROM "Menu" m
WHERE m."MenuCode" = 'BS_MANDATORY_VESSEL'
  AND NOT EXISTS (
    SELECT 1 FROM "LaunchPointTemplate" lpt
    WHERE lpt."TemplateName" = 'BS_ROLE_SYSADMIN' AND lpt."MenuID" = m."ID"
  );

-- Per-user grant: mirror the exact same audience that already has
-- BS_APPROVAL_RULES_V2 (BS_ROLE_SYSADMIN template users today).
INSERT INTO "LaunchPoint" (
    "UserName", "MenuCode", "MenuName", "Icon", "Route", "SortOrder",
    "IsActive", "Category", "MenuID", "TemplateName"
)
SELECT
    lp."UserName", 'BS_MANDATORY_VESSEL', m."MenuName", '', NULL, 14,
    true, 'MANAGEMENT', m."ID", 'BS_ROLE_SYSADMIN'
FROM "LaunchPoint" lp
CROSS JOIN "Menu" m
WHERE lp."MenuCode" = 'BS_APPROVAL_RULES_V2'
  AND m."MenuCode" = 'BS_MANDATORY_VESSEL'
  AND NOT EXISTS (
    SELECT 1 FROM "LaunchPoint" existing
    WHERE existing."UserName" = lp."UserName" AND existing."MenuCode" = 'BS_MANDATORY_VESSEL'
  );
