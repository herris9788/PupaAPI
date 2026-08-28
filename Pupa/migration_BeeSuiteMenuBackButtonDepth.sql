-- Replaces the short-lived Menu.HideBackButton bool (added last migration)
-- with Menu.BackButtonMinDepth (int): the minimum in-WebView navigation
-- depth before the AppBar's leading back arrow appears. 1 = always shown
-- (today's default everywhere); 2 = hidden only on the WebView's first
-- page, shown once the user navigates one level deeper; N = hidden until
-- depth reaches N. Strictly more general than the boolean it replaces.
--
-- Run against both beesuite and beesuite_staging.

ALTER TABLE "Menu" ADD COLUMN IF NOT EXISTS "BackButtonMinDepth" integer NOT NULL DEFAULT 1;

-- Backfill from the old bool before dropping it: HideBackButton=true meant
-- "never show" -> a depth no realistic navigation would reach.
UPDATE "Menu" SET "BackButtonMinDepth" = 999 WHERE "HideBackButton" = true;

ALTER TABLE "Menu" DROP COLUMN IF EXISTS "HideBackButton";
