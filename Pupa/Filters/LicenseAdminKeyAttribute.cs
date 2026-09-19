using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pupa.Filters
{
    // Gates the NetSuite Customer license/company *admin* endpoints behind a
    // shared secret (config "Licensing:AdminKey", sent as the X-Admin-Key
    // header). Unlike MenuController's admin endpoints — which sit behind the
    // gateway's X-App-Key — these mint and list license keys, so they must not
    // be usable by anyone who can merely reach the API. Fails closed: with no
    // key configured on the server every gated call is refused.
    //
    // Not applied to the public "activate" action the mobile app calls.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LicenseAdminKeyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var expected = context.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Licensing:AdminKey"];

            if (string.IsNullOrWhiteSpace(expected))
            {
                context.Result = new ObjectResult(new
                {
                    Error = "not_configured",
                    Message = "Licensing:AdminKey is not configured on the server."
                })
                { StatusCode = StatusCodes.Status503ServiceUnavailable };
                return;
            }

            var provided = context.HttpContext.Request.Headers["X-Admin-Key"].FirstOrDefault() ?? "";
            var ok = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected));

            if (!ok)
            {
                context.Result = new ObjectResult(new
                {
                    Error = "unauthorized",
                    Message = "Invalid or missing admin key."
                })
                { StatusCode = StatusCodes.Status401Unauthorized };
                return;
            }

            await next();
        }
    }
}
