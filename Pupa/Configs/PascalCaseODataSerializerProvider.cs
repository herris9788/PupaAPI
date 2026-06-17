using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Pupa.Configs
{
    public class PascalCaseODataResourceSerializer : ODataResourceSerializer
    {
        public PascalCaseODataResourceSerializer(IODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        public override ODataResource CreateResource(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            Debug.WriteLine($"[PascalCaseSerializer] CreateResource called. InstanceType={resourceContext?.ResourceInstance?.GetType().FullName}");

            var resource = base.CreateResource(selectExpandNode, resourceContext);
            if (resource == null || resource.Properties == null || resourceContext?.ResourceInstance == null)
            {
                return resource;
            }

            var instanceType = resourceContext.ResourceInstance.GetType();
            var props = new List<ODataProperty>(resource.Properties.Count());

            foreach (var prop in resource.Properties)
            {
                // read Name and Value using reflection (prop may be runtime-proxied)
                var nameInfo = prop?.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                var valueInfo = prop?.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);

                var originalName = nameInfo?.GetValue(prop) as string ?? string.Empty;
                var value = valueInfo?.GetValue(prop);

                // find CLR prop (ignore case) and use actual CLR name (PascalCase)
                var pi = !string.IsNullOrEmpty(originalName)
                    ? instanceType.GetProperty(originalName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                    : null;

                var name = pi != null ? pi.Name : originalName;

                // map nested values recursively so expanded resources also preserve PascalCase
                var mappedValue = MapValueToPascalCase(value);

                props.Add(new ODataProperty { Name = name, Value = mappedValue });
            }

            return new ODataResource
            {
                TypeName = resource.TypeName,
                Properties = props
            };
        }

        private object? MapValueToPascalCase(object? value)
        {
            if (value == null) return null;

            // ODataResource (single expanded entity)
            if (value is ODataResource nestedResource)
            {
                return MapResource(nestedResource);
            }

            // ODataCollectionValue (collection of primitives/complex/entity)
            if (value is ODataCollectionValue collectionValue)
            {
                // Map items: some items may be ODataResource instances
                var items = collectionValue.Items?.Select(i =>
                {
                    if (i is ODataResource orRes) return (object)MapResource(orRes);
                    return i;
                }).ToList();

                return new ODataCollectionValue
                {
                    TypeName = collectionValue.TypeName,
                    Items = items
                };
            }

            // Primitive or other value - return as-is
            return value;
        }

        private ODataResource MapResource(ODataResource source)
        {
            if (source == null || source.Properties == null) return source;

            var mappedProps = new List<ODataProperty>();

            foreach (var prop in source.Properties)
            {
                var nameInfo = prop?.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                var valueInfo = prop?.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);

                var originalName = nameInfo?.GetValue(prop) as string ?? string.Empty;
                var value = valueInfo?.GetValue(prop);

                // Try to locate CLR name by searching properties on the CLR type represented by TypeName
                string mappedName = originalName;
                try
                {
                    // TypeName looks like "Namespace.ModelType"; get CLR type name and find property by ignoring case
                    var typeName = source.TypeName;
                    if (!string.IsNullOrEmpty(typeName))
                    {
                        var clrTypeName = typeName.Split('.').Last();
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        var clrType = assemblies.Select(a => a.GetType(typeName) ?? a.GetTypes().FirstOrDefault(t => t.Name == clrTypeName)).FirstOrDefault(t => t != null);
                        if (clrType != null)
                        {
                            var pi = clrType.GetProperty(originalName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (pi != null) mappedName = pi.Name;
                        }
                    }
                }
                catch
                {
                    // ignore mapping failures and fall back to originalName
                }

                var mappedValue = MapValueToPascalCase(value);
                mappedProps.Add(new ODataProperty { Name = mappedName, Value = mappedValue });
            }

            return new ODataResource
            {
                TypeName = source.TypeName,
                Properties = mappedProps
            };
        }
    }

    public class PascalCaseODataSerializerProvider : ODataSerializerProvider
    {
        private readonly PascalCaseODataResourceSerializer _resourceSerializer;

        public PascalCaseODataSerializerProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _resourceSerializer = new PascalCaseODataResourceSerializer(this);
        }

        public override IODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType?.Definition == null)
            {
                return base.GetEdmTypeSerializer(edmType);
            }

            if (edmType.Definition.TypeKind == EdmTypeKind.Entity ||
                edmType.Definition.TypeKind == EdmTypeKind.Complex)
            {
                return _resourceSerializer;
            }

            return base.GetEdmTypeSerializer(edmType);
        }
    }
}