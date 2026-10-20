using Pupa.BusinessObjects.Beesuite;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Pupa.Configs
{
    /// <summary>
    /// Builds the OData EDM model from the registered entity types
    /// (replacement for DevExpress XAF's EdmModelBuilder).
    /// </summary>
    public static class BeesuiteEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            foreach (Type entityType in PupaEntityList.EntityTypes)
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

            // LSAItem (key gabungan ItemID+SupplierID) dilayani LSAItemController sendiri.
            var lsa = builder.AddEntityType(typeof(LSAItem));
            builder.AddEntitySet(nameof(LSAItem), lsa);

            return builder.GetEdmModel();
        }
    }
}
