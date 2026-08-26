-- BeeSuite mobile Quick Access: adds a platform permission (AllowWeb /
-- AllowMobile) to the existing Menu catalog so the mobile "Quick Access"
-- grid can be driven by the same Menu/LaunchPointTemplate/LaunchPoint
-- tables the web sidebar already uses, instead of being hardcoded per-role
-- in main.dart. See migration_BeeSuiteMenu.sql for the original setup this
-- extends.
--
-- Idempotent throughout (WHERE NOT EXISTS / ADD COLUMN IF NOT EXISTS) so
-- it's safe to re-run regardless of what any earlier ad-hoc grant already
-- did (e.g. the 7 example pages wired mid-session before this file existed).
--
-- Run against both beesuite (prod) and beesuite_staging via psql, same
-- workflow used all session.

BEGIN;

-- 1) New columns ---------------------------------------------------------
ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "AllowWeb" boolean NOT NULL DEFAULT true;
ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "AllowMobile" boolean NOT NULL DEFAULT false;

-- 2) New menu: "Manuals" (mobile-only PDF reader, never was on the web
--    sidebar, so it doesn't exist in the catalog at all yet) -------------
INSERT INTO "Menu" ("MenuCode","MenuName","Category","Route","SortOrder","IsActive")
SELECT 'BS_MANUALS','Manuals','OTHERS',null,3,true
WHERE NOT EXISTS (SELECT 1 FROM "Menu" WHERE "MenuCode" = 'BS_MANUALS');

-- 3) Mark every menu currently shown on the hardcoded mobile Quick Access
--    grid (verified against lib/main.dart's HomeTab, all 13 roles) as
--    AllowMobile = true, so switching the grid over to reading this column
--    reproduces exactly what's shown today before any admin touches the
--    new toggle. Everything else defaults to web-only (AllowMobile=false),
--    e.g. BS_PART_BOOK, BS_QUICK_ORDER, the various *_ADMIN/system pages.
UPDATE "Menu" SET "AllowMobile" = true
WHERE "MenuCode" IN (
  'BS_USER_MANAGEMENT','BS_APPROVAL_RULES','BS_ITEM_MANAGEMENT','BS_VESSEL_MANAGEMENT','BS_STOCK_CATEGORY',
  'BS_REPAIR_JOB_REQUEST','BS_OUTSTANDING_SERVICE_ATTENDANCE','BS_REPAIR_SERVICE_ORDERS','BS_MOBILIZATION',
  'BS_ATTENDANCE_TRIP','BS_TECHNICIAN_CALENDAR','BS_SERVICE_REPORT','BS_MY_SERVICE_REPORT',
  'BS_ITEM_REQUEST','BS_JOB_REQUEST','BS_BUNKER_REQUEST','BS_SHIPMENT_REQUEST','BS_TRACK_ITEM','BS_FEEDBACK',
  'BS_MANUALS','BS_APPROVALS','BS_COA_MANAGEMENT','BS_WHATSAPP_DEVICE',
  'BS_RFW_EXAMPLE','BS_RFW_APPROVAL_EXAMPLE','BS_JSON_DYNAMIC_WIDGET_EXAMPLE','BS_FLUTTER_HTML_EXAMPLE',
  'BS_FLUTTER_HTML_SERVER_DATA_EXAMPLE','BS_JSON_DYNAMIC_WIDGET_HOTPATCH_EXAMPLE','BS_WEBVIEW_EXAMPLE'
);

-- 4) Close the template gaps found by diffing the current mobile hardcoded
--    item list against the existing LaunchPointTemplate seed data ---------
INSERT INTO "LaunchPointTemplate" ("TemplateName","MenuID","Name","SortOrder","IsActive","IsComingSoon")
SELECT v.tmpl, m."ID", m."MenuName", v.sort, true, v.soon
FROM (VALUES
  -- ADMIN: mobile shows a "Job Request" tile (Soon-locked like every other
  -- role's Job Request) and the 7 example pages added earlier this session.
  ('BS_ROLE_ADMIN','BS_JOB_REQUEST',11,true::boolean),
  ('BS_ROLE_ADMIN','BS_RFW_EXAMPLE',12,NULL),
  ('BS_ROLE_ADMIN','BS_RFW_APPROVAL_EXAMPLE',13,NULL),
  ('BS_ROLE_ADMIN','BS_JSON_DYNAMIC_WIDGET_EXAMPLE',14,NULL),
  ('BS_ROLE_ADMIN','BS_FLUTTER_HTML_EXAMPLE',15,NULL),
  ('BS_ROLE_ADMIN','BS_FLUTTER_HTML_SERVER_DATA_EXAMPLE',16,NULL),
  ('BS_ROLE_ADMIN','BS_JSON_DYNAMIC_WIDGET_HOTPATCH_EXAMPLE',17,NULL),
  ('BS_ROLE_ADMIN','BS_WEBVIEW_EXAMPLE',18,NULL),
  -- VESSEL: mobile shows a "Repair Service Orders" tile.
  ('BS_ROLE_VESSEL','BS_REPAIR_SERVICE_ORDERS',4,NULL),
  -- Manuals: granted to every role whose mobile grid shows it today.
  ('BS_ROLE_REQUESTER','BS_MANUALS',10,NULL),
  ('BS_ROLE_REQUESTER_ONLINE','BS_MANUALS',11,NULL),
  ('BS_ROLE_ADMIN_TESTING','BS_MANUALS',8,NULL),
  ('BS_ROLE_CREW','BS_MANUALS',6,NULL)
) AS v(tmpl, code, sort, soon)
JOIN "Menu" m ON m."MenuCode" = v.code
WHERE NOT EXISTS (
  SELECT 1 FROM "LaunchPointTemplate" lpt
  WHERE lpt."TemplateName" = v.tmpl AND lpt."MenuID" = m."ID"
);

-- 5) Backfill every EXISTING user from their role's (now-updated) template,
--    so nobody's Quick Access changes the moment main.dart switches over to
--    reading this — same pattern as migration_BeeSuiteMenu.sql's step 4.
INSERT INTO "LaunchPoint" ("UserName","MenuID","MenuCode","MenuName","Category","SortOrder","IsActive","IsComingSoon","TemplateName")
SELECT u."Username", m."ID", m."MenuCode", m."MenuName", m."Category", lpt."SortOrder", true, lpt."IsComingSoon", lpt."TemplateName"
FROM "User" u
JOIN "LaunchPointTemplate" lpt ON lpt."TemplateName" = 'BS_ROLE_' || upper(u."Role")
JOIN "Menu" m ON m."ID" = lpt."MenuID"
WHERE NOT EXISTS (
  SELECT 1 FROM "LaunchPoint" lp2 WHERE lp2."UserName" = u."Username" AND lp2."MenuID" = m."ID"
);

COMMIT;
