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
ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricEnabledAt"   timestamp    NULL;
ALTER TABLE "User" ADD COLUMN IF NOT EXISTS "BiometricLastUsedAt"  timestamp    NULL;
