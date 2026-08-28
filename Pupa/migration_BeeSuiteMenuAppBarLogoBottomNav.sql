-- Adds Menu.AppBarShowLogo and Menu.ShowBottomNav: two more per-menu
-- WebPage-embedding options alongside HideAppBar (migration_
-- BeeSuiteMenuHideAppBar.sql). Both default false, so every existing menu
-- keeps rendering exactly as today.
--
-- AppBarShowLogo: when the AppBar is shown (HideAppBar=false), show the
-- BeeSuite logo instead of the menu's title text.
-- ShowBottomNav: also render the app's native floating bottom nav bar on
-- top of the embedded WebPage.
--
-- Run against both beesuite and beesuite_staging.

ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "AppBarShowLogo" boolean NOT NULL DEFAULT false;
ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "ShowBottomNav" boolean NOT NULL DEFAULT false;
