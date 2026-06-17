using Microsoft.OpenApi.Models;

namespace Pupa.Configs
{
    /// <summary>
    /// Parameter without body.
    /// </summary>
    public class NonBodyParameter : OpenApiParameter
    {
        /// <summary>
        /// Default.
        /// </summary>
        public object? Default { get; set; }
    }
}
