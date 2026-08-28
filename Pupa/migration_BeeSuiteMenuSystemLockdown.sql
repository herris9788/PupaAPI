-- Revokes access to every Menu in the "SYSTEM" category (Whatsapp Device,
-- System Logs, Cache Control, Requisition Admin, Job Request Admin, Job
-- Request Prune, Settings, and the 7 *_EXAMPLE pages) from every user
-- EXCEPT role SYSADMIN and username "jay" (case-insensitive) -- per explicit
-- request. SYSADMIN2 is intentionally NOT exempted (confirmed with user).
--
-- Also strips the per-role LaunchPointTemplate grants (every template
-- except BS_ROLE_SYSADMIN) so a newly created/synced user of any other role
-- doesn't silently get these back the next time AuthApi.syncDefaultMenuAccess
-- backfills their LaunchPoint from the template.
--
-- Run against both beesuite_staging and beesuite.

BEGIN;

DELETE FROM "LaunchPoint" lp
USING "Menu" m, "User" u
WHERE lp."MenuID" = m."ID"
  AND m."Category" = 'SYSTEM'
  AND lower(u."Username") = lower(lp."UserName")
  AND u."Role" <> 'SYSADMIN'
  AND lower(lp."UserName") <> 'jay';

DELETE FROM "LaunchPointTemplate" lpt
USING "Menu" m
WHERE lpt."MenuID" = m."ID"
  AND m."Category" = 'SYSTEM'
  AND lpt."TemplateName" <> 'BS_ROLE_SYSADMIN';

COMMIT;
