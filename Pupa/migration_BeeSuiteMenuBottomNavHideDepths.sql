-- Adds Menu.BottomNavHideDepths: a comma/range expression ("1", "3,5",
-- "4-10", "1,4-10") of in-WebView navigation depths at which the floating
-- bottom nav bar (Menu.ShowBottomNav) should be hidden. Null/empty = shown
-- on every page whenever ShowBottomNav is true (unchanged default
-- behavior for every existing menu).
--
-- Run against both beesuite and beesuite_staging.

ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "BottomNavHideDepths" text NULL;
