using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;

namespace Pupa.Configs
{
    public class OdataValidator : ODataQueryValidator
    {
        public override void Validate(ODataQueryOptions options, ODataValidationSettings validationSettings)
        {
            validationSettings.MaxExpansionDepth = 100;
            validationSettings.MaxNodeCount = 1000;
            base.Validate(options, validationSettings);
        }
    }
}
