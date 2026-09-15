-- Adds Base64 storage to JobRequestAttachment and JobAttachment so new Job
-- Request attachments (both document-level and per-item) can be created
-- self-contained via Pupa's own OData API, mirroring how the Attachment
-- entity already stores Requisition attachments. Legacy rows keep using
-- their existing PreviewUrl-based external storage (StorageProvider
-- 'attachment-api'/'s3') via the existing fallback fetch in every reader.
ALTER TABLE "JobRequestAttachment" ADD COLUMN IF NOT EXISTS "Base64" text NULL;
ALTER TABLE "JobAttachment"        ADD COLUMN IF NOT EXISTS "Base64" text NULL;
