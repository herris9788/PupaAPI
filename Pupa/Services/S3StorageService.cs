using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Pupa.Services
{
    /// <summary>
    /// S3-compatible object storage (Ceph/IDCloudHost), configured the same way as
    /// beesuite_go services/storage_service.go: path-style, checksums only when
    /// required, unsigned payload. Config section "S3": Endpoint, Region, Bucket,
    /// AccessKey, SecretKey. Empty Endpoint/Bucket = disabled (IsEnabled false).
    /// </summary>
    public class S3StorageService
    {
        private readonly AmazonS3Client? _client;
        private readonly string _bucket;
        private readonly string _objectUrlBase; // path-style: {Endpoint}/{Bucket}/

        public S3StorageService(IConfiguration config, ILogger<S3StorageService> logger)
        {
            var s = config.GetSection("S3");
            _bucket = s["Bucket"] ?? "";
            var endpoint = s["Endpoint"];
            _objectUrlBase = $"{endpoint?.TrimEnd('/')}/{_bucket}/";
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(_bucket))
                return;

            // A bad S3 config must only disable CreateV2, never break RequisitionController (V1 Create shares it).
            try
            {
                _client = new AmazonS3Client(
                    new BasicAWSCredentials(s["AccessKey"], s["SecretKey"]),
                    new AmazonS3Config
                    {
                        ServiceURL = endpoint,
                        AuthenticationRegion = string.IsNullOrWhiteSpace(s["Region"]) ? "us-east-1" : s["Region"],
                        ForcePathStyle = true, // required for IDCloudHost / MinIO
                        RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                        ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "S3 storage disabled: invalid \"S3\" configuration.");
            }
        }

        public bool IsEnabled => _client != null;

        /// <summary>Path-style object URL, e.g. https://is3.cloudhost.id/waki-storage/requisition/x.pdf</summary>
        public string ObjectUrl(string key) => _objectUrlBase + key;

        /// <summary>Object key from a URL made by ObjectUrl; null when the URL is not in this bucket.</summary>
        public string? KeyFromUrl(string? url)
            => url != null && url.StartsWith(_objectUrlBase, StringComparison.OrdinalIgnoreCase) ? url[_objectUrlBase.Length..] : null;

        public async Task UploadAsync(string key, byte[] bytes, string contentType, CancellationToken ct = default)
        {
            using var stream = new MemoryStream(bytes);
            await Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                DisablePayloadSigning = true, // Ceph rejects the SDK's signed/chunked payload
                UseChunkEncoding = false,
            }, ct);
        }

        public async Task<GetObjectResponse> GetAsync(string key, CancellationToken ct = default)
            => await Client.GetObjectAsync(_bucket, key, ct);

        public async Task DeleteAsync(string key, CancellationToken ct = default)
            => await Client.DeleteObjectAsync(_bucket, key, ct);

        private AmazonS3Client Client => _client ?? throw new InvalidOperationException("S3 storage is not configured (section \"S3\").");
    }
}
