using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Pupa.Configs
{
    /// <summary>
    /// Renames each closed generic <see cref="CustomDataController{TEntity}"/> to
    /// its entity type name so ASP.NET Core OData routing maps it to the matching
    /// entity set (replacement for DevExpress XAF's generic controller routing).
    ///
    /// This is implemented as an <see cref="IApplicationModelProvider"/> rather
    /// than an <see cref="IControllerModelConvention"/> because OData's routing
    /// runs as an application-model provider (OnProvidersExecuted). The rename
    /// must therefore happen in OnProvidersExecuting — which completes for every
    /// provider before any OnProvidersExecuted runs — so OData sees the new names.
    /// </summary>
    public class GenericControllerNameProvider : IApplicationModelProvider
    {
        // After DefaultApplicationModelProvider (-1000), before OData's provider.
        public int Order => -100;

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var controller in context.Result.Controllers)
            {
                var type = controller.ControllerType;
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(CustomDataController<>))
                {
                    controller.ControllerName = type.GenericTypeArguments[0].Name;
                }
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }
    }

    /// <summary>
    /// Builds the OData EDM model from the registered entity types
    /// (replacement for DevExpress XAF's EdmModelBuilder).
    /// </summary>
    public static class BeesuiteEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            foreach (Type entityType in CustomGenericControllerFeatureProvider.EntityTypes)
            {
                var et = builder.AddEntityType(entityType);
                builder.AddEntitySet(entityType.Name, et);
            }

            // Item is served by ItemODataController, not the generic controller,
            // but still needs to be registered in the EDM model for OData routing.
            var itemEntity = builder.AddEntityType(typeof(Item));
            builder.AddEntitySet(nameof(Item), itemEntity);

            // InventoryUserGroup is served by its own controller but still needs
            // to be exposed as an entity set in the EDM model.
            var iug = builder.AddEntityType(typeof(InventoryUserGroup));
            builder.AddEntitySet(nameof(InventoryUserGroup), iug);

            return builder.GetEdmModel();
        }
    }
}
