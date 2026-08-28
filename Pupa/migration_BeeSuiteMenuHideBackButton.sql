-- Adds Menu.HideBackButton: one more per-menu WebPage option alongside
-- HideAppBar/AppBarShowLogo/ShowBottomNav. Default false, so every existing
-- menu keeps showing the AppBar's leading back arrow exactly as today.
--
-- Run against both beesuite and beesuite_staging.

ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "HideBackButton" boolean NOT NULL DEFAULT false;
