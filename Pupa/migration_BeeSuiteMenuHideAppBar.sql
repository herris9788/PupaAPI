-- Adds Menu.HideAppBar: per-menu toggle so the mobile WebPage embedding
-- that menu's Route can render without its own AppBar (for embedded pages
-- that already provide their own header/back control). Defaults false —
-- every existing menu keeps showing the WebPage AppBar exactly as today.
--
-- Run against both beesuite and beesuite_staging.

ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "HideAppBar" boolean NOT NULL DEFAULT false;
