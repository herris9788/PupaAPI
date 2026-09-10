-- ============================================================================
-- Registers the "Group Approvers" admin page (Approval Rule V2, Group-based
-- tier -- manages UserApprovalGroup) in the DB-driven Menu/LaunchPoint
-- system. Same pattern as migration_MandatoryVesselMenu.sql: a Menu row,
-- a BS_ROLE_SYSADMIN LaunchPointTemplate grant, and a per-user LaunchPoint
-- for the exact audience that already has BS_APPROVAL_RULES_V2.
-- PostgreSQL. Run once against beesuite (live) and beesuite_staging.
-- ============================================================================

INSERT INTO "Menu" (
    "MenuCode", "MenuName", "Icon", "Route", "Category", "Description",
    "SortOrder", "IsActive", "IsLogistic", "IsComingSoon",
    "AllowWeb", "AllowMobile", "HideAppBar", "AppBarShowLogo",
    "ShowBottomNav", "BackButtonMinDepth"
)
SELECT
    'BS_GROUP_APPROVERS', 'Group Approvers', '', '/group-approvers', 'MANAGEMENT', '',
    14, true, false, false,
    true, false, false, false,
    false, 1
WHERE NOT EXISTS (SELECT 1 FROM "Menu" WHERE "MenuCode" = 'BS_GROUP_APPROVERS');

INSERT INTO "LaunchPointTemplate" ("TemplateName", "MenuID", "Name", "Icon", "SortOrder", "IsActive", "IsLogistic")
SELECT 'BS_ROLE_SYSADMIN', m."ID", m."MenuName", '', 14, true, false
FROM "Menu" m
WHERE m."MenuCode" = 'BS_GROUP_APPROVERS'
  AND NOT EXISTS (
    SELECT 1 FROM "LaunchPointTemplate" lpt
    WHERE lpt."TemplateName" = 'BS_ROLE_SYSADMIN' AND lpt."MenuID" = m."ID"
  );

INSERT INTO "LaunchPoint" (
    "UserName", "MenuCode", "MenuName", "Icon", "Route", "SortOrder",
    "IsActive", "Category", "MenuID", "TemplateName"
)
SELECT
    lp."UserName", 'BS_GROUP_APPROVERS', m."MenuName", '', NULL, 14,
    true, 'MANAGEMENT', m."ID", 'BS_ROLE_SYSADMIN'
FROM "LaunchPoint" lp
CROSS JOIN "Menu" m
WHERE lp."MenuCode" = 'BS_APPROVAL_RULES_V2'
  AND m."MenuCode" = 'BS_GROUP_APPROVERS'
  AND NOT EXISTS (
    SELECT 1 FROM "LaunchPoint" existing
    WHERE existing."UserName" = lp."UserName" AND existing."MenuCode" = 'BS_GROUP_APPROVERS'
  );
