-- ============================================================================
-- Add an "ApprovalRuleVersion" column to Requisition, snapshotting which
-- approval-scope table drove routing for THAT document at the moment it was
-- created (1 = old UserApprovalScope cascade, 2 = newer Specificity-based
-- UserApprovalScope2 — same meaning as InventoryUser.ApprovalRuleVersion).
--
-- NOT NULL DEFAULT 1 so every existing row is backfilled to 1 automatically
-- (old data always reads as Approval Rule 1, unchanged behavior). New rows
-- get an explicit 1 or 2 from BeesuiteDbContext.SaveChanges, which copies the
-- vessel's CURRENT ApprovalRuleVersion at insert time — locking each document
-- to whichever rule engine was active when it was submitted, so changing a
-- vessel's flag later never retroactively re-routes an already-submitted
-- requisition.
-- PostgreSQL. Run once against the Beesuite database.
-- ============================================================================

ALTER TABLE "Requisition"
    ADD COLUMN IF NOT EXISTS "ApprovalRuleVersion" integer NOT NULL DEFAULT 1;
