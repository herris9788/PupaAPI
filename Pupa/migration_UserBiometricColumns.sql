-- ============================================================================
-- Biometric login (stored device-token scheme) — add columns to "User".
-- PostgreSQL. Run once against the Beesuite database.
-- Schema is managed manually (no EF migrations).
--
-- One biometric device per user. Biometric data NEVER reaches the server: the
-- device keeps a random token in its secure storage behind the fingerprint/face
-- prompt, and only the SHA-256 HASH of that token is stored in
-- "BiometricTokenHash". Clear that column to revoke.
-- ============================================================================

ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricTokenHash"   varchar(128) NULL;
ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricDeviceID"    varchar(200) NULL;
ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricDeviceName"  varchar(200) NULL;
ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricPlatform"    varchar(20)  NULL;
-- timestamptz (WITH time zone), matching the other DateTime columns on "User"
-- (RefreshTokenExpiryUtc, SuspendedUntil). The ApiGateway runs Npgsql in
-- non-legacy mode where a DateTime maps to 'timestamp with time zone' and must
-- be Kind=Utc; a plain 'timestamp' column reads back as Kind=Unspecified and
-- makes EF's full-row UPDATE throw, so these MUST be timestamptz.
ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricEnabledAt"   timestamptz  NULL;
ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricLastUsedAt"  timestamptz  NULL;

-- If you already created these as plain 'timestamp' (an earlier version of this
-- script), convert them in place (existing values are interpreted as UTC):
ALTER TABLE "User" ALTER COLUMN "BiometricEnabledAt"  TYPE timestamptz USING "BiometricEnabledAt"  AT TIME ZONE 'UTC';
ALTER TABLE "User" ALTER COLUMN "BiometricLastUsedAt" TYPE timestamptz USING "BiometricLastUsedAt" AT TIME ZONE 'UTC';
