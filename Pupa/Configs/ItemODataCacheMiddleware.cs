using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;

namespace Pupa.Configs
{
    public class ItemODataCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private const string KeyPrefix = "beesuite:odata:";

        public ItemODataCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
        {
            if (!IsItemODataGet(context))
            {
                await _next(context);
                return;
            }

            var cacheKey = KeyPrefix + HashKey(context.Request.Path + context.Request.QueryString);

            var cached = await cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json; odata.metadata=minimal; odata.streaming=true; charset=utf-8";
                await context.Response.WriteAsync(cached);
                return;
            }

            var originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await _next(context);

                buffer.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(buffer).ReadToEndAsync();

                if (context.Response.StatusCode == 200 && !string.IsNullOrEmpty(body))
                {
                    await cache.SetStringAsync(cacheKey, body, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                    });
                }

                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private static readonly string[] _cachedODataPaths =
        [
            "/beesuite/odata/Item",
            "/beesuite/odata/LaunchPoint",
        ];

        private static bool IsItemODataGet(HttpContext context)
            => context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
            && _cachedODataPaths.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

        private static string HashKey(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes)[..20];
        }
    }
}
