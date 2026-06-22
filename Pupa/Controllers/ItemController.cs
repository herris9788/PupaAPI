using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Pupa.Configs;

namespace Pupa.Controllers
{
    /// <summary>
    /// Dedicated OData controller for the Item entity set.
    /// Overrides the generic <see cref="CustomDataController{Item}"/> so we can
    /// intercept the optional <c>vesselId</c> query parameter and pre-filter
    /// paint items (T06.001 prefix) to only those allowed for the vessel's paint group.
    /// Non-paint items are never filtered out.
    /// </summary>
    public class ItemController : ODataController
    {
        private readonly BeesuiteDbContext _db;

        public ItemController(BeesuiteDbContext db)
        {
            _db = db;
        }

        // GET /beesuite/odata/Item
        // Optional query params:
        //   vesselId=<int>        — required to enable paint filter
        //   paintFilter=true      — must be present alongside vesselId to activate the filter
        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public async Task<IActionResult> Get()
        {
            IQueryable<Item> query = _db.Set<Item>();

            var paintFilterRequested = Request.Query.TryGetValue("paintFilter", out var pfVal)
                && pfVal.ToString().Equals("true", StringComparison.OrdinalIgnoreCase);

            if (paintFilterRequested &&
                Request.Query.TryGetValue("vesselId", out var vesselIdStr) &&
                int.TryParse(vesselIdStr, out int vesselId))
            {
                var allowedCodes = await ResolvePaintAllowedCodesAsync(vesselId);
                if (allowedCodes != null)
                    query = ApplyPaintFilter(query, allowedCodes);
            }

            return Ok(query);
        }

        // GET /beesuite/odata/Item(key)
        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public IActionResult Get(int key)
        {
            var entity = _db.Set<Item>().Find(key);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private async Task<IReadOnlyList<string>?> ResolvePaintAllowedCodesAsync(int vesselId)
        {
            var vessel = await _db.Set<InventoryUser>()
                .Where(u => u.ID == vesselId)
                .Select(u => new { u.InventoryUserName, u.GroupID, u.DB })
                .FirstOrDefaultAsync();

            if (vessel == null) return null;

            var groupCode = await _db.Set<InventoryUserGroup>()
                .Where(g => g.GroupID == vessel.GroupID && g.DB == vessel.DB)
                .Select(g => g.GroupCode)
                .FirstOrDefaultAsync();

            var groupKey = PaintPolicy.ResolveGroup(vessel.InventoryUserName ?? "", groupCode);
            return PaintPolicy.AllowedCodes(groupKey);
        }

        private static IQueryable<Item> ApplyPaintFilter(IQueryable<Item> query, IReadOnlyList<string> allowedCodes)
        {
            var prefix = PaintPolicy.PaintItemCodePrefix;
            var codeList = allowedCodes.ToList();
            return query.Where(i =>
                i.ItemCode == null ||
                !i.ItemCode.StartsWith(prefix) ||
                codeList.Contains(i.ItemCode));
        }
    }
}
