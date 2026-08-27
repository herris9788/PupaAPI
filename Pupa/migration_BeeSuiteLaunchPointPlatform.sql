-- Per-user platform override for BeeSuite menu grants: an admin can grant a
-- user a menu on mobile only (or web only), even when the menu itself is
-- globally allowed on both (Menu.AllowWeb/AllowMobile) — mirrors the
-- existing LaunchPoint.IsComingSoon override pattern exactly. NULL = inherit
-- the menu's global default.
--
-- Run against both beesuite (prod) and beesuite_staging via psql, same
-- workflow used all session.

BEGIN;

ALTER TABLE "LaunchPoint" ADD COLUMN IF NOT EXISTS "AllowWeb" boolean NULL;
ALTER TABLE "LaunchPoint" ADD COLUMN IF NOT EXISTS "AllowMobile" boolean NULL;

COMMIT;
