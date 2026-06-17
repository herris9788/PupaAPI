using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pupa.Configs
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan global.");

                // Atur response
                //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //context.Response.ContentType = "application/json";

                var Message = "";
                if (ex.InnerException != null)
                {
                    Message = ex.InnerException.Message;
                }
                else
                {
                    Message = ex.Message;
                }

                var errorResponse = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message // Pesan ramah untuk pengguna
                            // Anda mungkin ingin menambahkan detail error hanya dalam lingkungan pengembangan
                            // Detail = ex.Message
                };

                var jsonResponse = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}
