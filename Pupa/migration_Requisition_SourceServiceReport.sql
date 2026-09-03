-- ============================================================================
-- Requisition.Source{ServiceReport,JobRequest}{No,ID} — explicit link back to
-- the Service Report / Job Request that auto-created this Item Request.
--
-- Replaces parsing the reference out of Requisition.Remarks free-text, which
-- broke once ServiceReport.ReportNo / JobRequest.ReportNo gained a vessel-code
-- segment (e.g. "SRBERG26080003"). Powers the "Related Documents" panel on the
-- Review & Approve IR page (BeesuiteGO GET /go/requisition/:id/related-documents).
--
-- PostgreSQL. Run once against beesuite_staging, verify, then beesuite. Schema
-- is managed manually (no EF migrations), matching the existing convention in
-- this repo. Safe to re-run (all steps are idempotent).
--
-- DEPLOY ORDER: this migration FIRST, then PupaAPI, then BeesuiteGO / Flutter.
-- ============================================================================

BEGIN;

-- ── 1. Columns + indexes ────────────────────────────────────────────────────
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "SourceServiceReportNo" varchar(200) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "SourceServiceReportID" integer NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "SourceJobRequestNo"    varchar(200) NULL;
ALTER TABLE "Requisition" ADD COLUMN IF NOT EXISTS "SourceJobRequestID"    integer NULL;

CREATE INDEX IF NOT EXISTS "idx_requisition_source_service_report_no"
    ON "Requisition" ("SourceServiceReportNo");
CREATE INDEX IF NOT EXISTS "idx_requisition_source_job_request_no"
    ON "Requisition" ("SourceJobRequestNo");

-- ── 2. Backfill Service Report links from Remarks ────────────────────────────
-- Covers both the old digits-only numbers ("SR26070009") and the newer
-- "SR<VesselCode><YY><MM><N4>" form ("SRBERG26080003").
UPDATE "Requisition"
SET "SourceServiceReportNo" = (regexp_match("Remarks", 'from Service Report (SR[A-Za-z0-9]+)'))[1]
WHERE "SourceServiceReportNo" IS NULL
  AND "Remarks" ~ 'from Service Report SR[A-Za-z0-9]+';

-- Resolve the matching ServiceReport.ID where the number lines up.
UPDATE "Requisition" r
SET "SourceServiceReportID" = sr."ID"
FROM "ServiceReport" sr
WHERE r."SourceServiceReportID" IS NULL
  AND r."SourceServiceReportNo" IS NOT NULL
  AND sr."DeletedAt" IS NULL
  AND UPPER(TRIM(sr."ReportNo")) = UPPER(TRIM(r."SourceServiceReportNo"));

-- ── 3. Backfill Job Request links from LinkedRequisitions (EAV) ──────────────
-- The reverse link lives in JobRequest.AdditionalData.LinkedRequisitions, stored
-- as a JobFieldValue row (EntityType='JobRequest', FieldKey='LinkedRequisitions',
-- ValueJson = array of {RequisitionID, RequisitionNumber, ...}).
UPDATE "Requisition" r
SET "SourceJobRequestID" = jr."ID",
    "SourceJobRequestNo" = jr."ReportNo"
FROM "JobFieldValue" jfv
JOIN "JobRequest" jr
     ON jr."ID" = jfv."EntityID"
    AND jr."DeletedAt" IS NULL
CROSS JOIN LATERAL jsonb_array_elements(jfv."ValueJson") AS elem
WHERE r."SourceJobRequestID" IS NULL
  AND jfv."EntityType" = 'JobRequest'
  AND jfv."FieldKey"   = 'LinkedRequisitions'
  AND jfv."ValueType"  = 'json'
  AND jsonb_typeof(jfv."ValueJson") = 'array'
  AND (elem->>'RequisitionID') ~ '^\d+$'
  AND (elem->>'RequisitionID')::int = r."ID";

COMMIT;

-- ── Verification (run manually after COMMIT) ─────────────────────────────────
-- SELECT
--   count(*) FILTER (WHERE "SourceServiceReportNo" IS NOT NULL) AS sr_linked,
--   count(*) FILTER (WHERE "SourceServiceReportID" IS NOT NULL) AS sr_id_resolved,
--   count(*) FILTER (WHERE "SourceJobRequestNo"    IS NOT NULL) AS jr_linked
-- FROM "Requisition";
