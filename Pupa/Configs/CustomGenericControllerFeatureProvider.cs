using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Deltas;
using System.Reflection;

namespace Pupa.Configs
{
    public class CustomGenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public static readonly Type[] EntityTypes = new Type[] {
            // Item is handled by ItemODataController (supports vesselId paint filter)
            typeof(UOM),
            typeof(ItemCategory),
            typeof(Specification),
            typeof(User),
            typeof(UserVesselRel),
            typeof(Requisition),
            typeof(RequisitionDetail),
            typeof(VesselSpecRel),
            typeof(StockCategory),
            typeof(StockFamily),
            typeof(StockFamilyCOA),
            typeof(InventoryUser),
            typeof(InventoryUser),
            typeof(Approval),
            typeof(Attachment),
            typeof(RequisitionAttachmentRel),
            typeof(Event),
            typeof(EventHamperItem),
            typeof(EventParticipant),
            typeof(EventUserSpecificItem),
            typeof(UserV2),
              typeof(UserV3),
            typeof(Notification),
            typeof(NotificationRead),
            typeof(InventoryUserSpec),
            typeof(DocumentNumbering),
            typeof(ROB),
            typeof(PartBook),
            typeof(Job),
            typeof(JobDetail),
            typeof(JobRequest),
            typeof(JobFieldDefinition),
            typeof(UserApprovalScope),
            typeof(LogActivity),
            typeof(RequisitionEngineNumber),
            typeof(RequisitionCylinderNumber),
            typeof(RequisitionDetailAttachmentRel),
            typeof(WhatsappDevice),
            typeof(LaunchPoint),
            typeof(Menu),
            typeof(LaunchPointTemplate),
            typeof(UserPermission),
            typeof(OrgDelegation),
            typeof(UserApprovalDelegation),
            typeof(OrgLevel),
            typeof(OrgPosition),
            typeof(OrgDepartment),
            typeof(OrgPositionDepartment),
            typeof(TemplatePermission),
            typeof(MenuFeature),
            typeof(ErrorLog),
            typeof(AccessLog),
            typeof(WhatsappDeviceGroup),
            typeof(AppConfig),
            typeof(JobAttachment),
            typeof(JobRequestAttachment),
            typeof(UserApprovalDelegation),
            typeof(Item)
        };
        public static readonly Type[] EntityTypesExternal = new Type[] {
          //typeof(SH_01), typeof(SH_02)
        };

        private readonly IServiceCollection serviceCollection;
        public CustomGenericControllerFeatureProvider(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (Type entityType in EntityTypes)
            {
                // create controller TypeInfo and get underlying Type for robust equality checks
                var controllerTypeInfo = typeof(CustomDataController<>)
                    .MakeGenericType(entityType)
                    .GetTypeInfo();
                var controllerType = controllerTypeInfo.AsType();

                // Avoid adding duplicate controller types which cause Swagger to see duplicate method/path combos
                // Compare underlying Type rather than TypeInfo instances
                if (!feature.Controllers.Any(t => t.AsType() == controllerType))
                {
                    feature.Controllers.Add(controllerTypeInfo);
                }
            }
            //foreach (Type ent in EntityTypesExternal)
            //{
            //  if(ent == typeof(En))
            //  {
            //    Type deltaType = typeof(Delta<>).MakeGenericType(ent);
            //    feature.Controllers.Add(typeof(UserCon<,>).MakeGenericType(ent, deltaType).GetTypeInfo());
            //
            //  }
            //}
        }
    }
}
