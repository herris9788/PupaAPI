-- Fix: "Item Request" / "Job Request" mobile Quick Access placeholder tiles
-- disappeared instead of showing disabled with a "Soon" badge like they did
-- under the old hardcoded per-role cascade (lib/main.dart's _buildMenuCard
-- still has the hardcoded TitleText == "Item Request" / "Job Request" Soon
-- gate -- unrelated to this data -- but the dynamic grid never renders a
-- tile at all unless the user actually has a granted LaunchPoint row with
-- AllowMobile true, so a missing grant/AllowMobile hides the tile outright
-- instead of showing it disabled).
--
-- Root cause found by comparing the DB against the old cascade (removed in
-- the same change that added _buildQuickAccessTiles to main.dart):
--   1) Menu.AllowMobile ended up false for BS_ITEM_REQUEST/BS_JOB_REQUEST on
--      both beesuite and beesuite_staging, even though
--      migration_BeeSuiteMenuMobile.sql's step 3 listed both codes in its
--      AllowMobile=true UPDATE -- something toggled it back off afterwards
--      (most likely a Menu Management "Mobile" toggle while testing).
--   2) LaunchPointTemplate never had a BS_JOB_REQUEST row for
--      BS_ROLE_REQUESTER or BS_ROLE_ADMIN_TESTING, even though the old
--      cascade showed a (Soon-disabled) "Job Request" tile for both roles
--      (REQUESTER unconditionally, ADMIN_TESTING via the
--      REQUESTER_ONLINE/ADMIN_TESTING branch) -- only BS_ROLE_ADMIN had it.
--
-- Idempotent throughout, safe to re-run. Run against both beesuite and
-- beesuite_staging.

BEGIN;

-- 1) Restore AllowMobile so these tiles render again (still disabled by the
--    hardcoded title check in _buildMenuCard, so this can't make them
--    tappable).
UPDATE "Menu" SET "AllowMobile" = true
WHERE "MenuCode" IN ('BS_ITEM_REQUEST', 'BS_JOB_REQUEST');

-- 2) Close the two missing per-role template grants for BS_JOB_REQUEST.
INSERT INTO "LaunchPointTemplate" ("TemplateName","MenuID","Name","SortOrder","IsActive","IsComingSoon")
SELECT v.tmpl, m."ID", m."MenuName", v.sort, true, true
FROM (VALUES
  ('BS_ROLE_REQUESTER','BS_JOB_REQUEST',12),
  ('BS_ROLE_ADMIN_TESTING','BS_JOB_REQUEST',9)
) AS v(tmpl, code, sort)
JOIN "Menu" m ON m."MenuCode" = v.code
WHERE NOT EXISTS (
  SELECT 1 FROM "LaunchPointTemplate" lpt
  WHERE lpt."TemplateName" = v.tmpl AND lpt."MenuID" = m."ID"
);

-- 3) Backfill every EXISTING user from their role's (now-updated) template,
--    same pattern as migration_BeeSuiteMenuMobile.sql's step 5.
INSERT INTO "LaunchPoint" ("UserName","MenuID","MenuCode","MenuName","Category","SortOrder","IsActive","IsComingSoon","TemplateName")
SELECT u."Username", m."ID", m."MenuCode", m."MenuName", m."Category", lpt."SortOrder", true, lpt."IsComingSoon", lpt."TemplateName"
FROM "User" u
JOIN "LaunchPointTemplate" lpt ON lpt."TemplateName" = 'BS_ROLE_' || upper(u."Role")
JOIN "Menu" m ON m."ID" = lpt."MenuID"
WHERE NOT EXISTS (
  SELECT 1 FROM "LaunchPoint" lp2 WHERE lp2."UserName" = u."Username" AND lp2."MenuID" = m."ID"
);

COMMIT;
