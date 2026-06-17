using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Pupa.Configs
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            //if(context.ApiDescription.HttpMethod == "GET")
            //{
            //operation.Parameters.Add(new OpenApiParameter
            //{
            //    Name = "X-API-DB",
            //    In = ParameterLocation.Header,
            //    Required = false
            //});
            //}
            if (context.MethodInfo.Name == "Get")
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$select",
                    Description = "Select property.",
                    Required = false,
                    In = ParameterLocation.Query
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$filter",
                    Description = "Filter the results using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$orderby",
                    Description = "Order the results using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$skip",
                    Description = "The number of results to skip.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$top",
                    Description = "The number of results to return.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$count",
                    Description = "Return the total count.",
                    Required = false,
                    In = ParameterLocation.Query
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$expand",
                    Description = "Expand children.",
                    Required = false,
                    In = ParameterLocation.Query
                });
            }

        }

    }
}
