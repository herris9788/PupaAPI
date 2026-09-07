using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;

namespace Pupa.Controllers
{
    /// <summary>
    /// Plain (non-OData) custom actions for Item. Route is hardcoded to
    /// "beesuite/api/Item" rather than using the "[controller]" token because
    /// the class can't be named ItemController — that name is already taken by
    /// the dedicated OData controller for the Item entity set
    /// (Controllers/ItemController.cs, route beesuite/odata/Item).
    /// </summary>
    [Route("beesuite/api/Item")]
    public class ItemApiController : Controller
    {
        private readonly BeesuiteDbContext db;

        public ItemApiController(BeesuiteDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Every item equivalent to [itemCode] — rows sharing the same
        /// GroupID in ItemEquivalent (e.g. the same lubricant sold under
        /// different brands), INCLUDING [itemCode] itself so the caller gets
        /// the whole group to render/filter as needed.
        /// </summary>
        [HttpGet("Equivalent")]
        public async Task<IActionResult> Equivalent([FromQuery] string? itemCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new Exception("itemCode is required");

                var GroupIDs = await db.ItemEquivalent.AsNoTracking()
                    .Where(x => x.ItemCode == itemCode)
                    .Select(x => x.GroupID)
                    .Distinct()
                    .ToListAsync();

                if (!GroupIDs.Any())
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = $"No equivalent group found for itemCode '{itemCode}'.",
                        Data = (object?)null
                    });
                }

                var Items = await db.ItemEquivalent.AsNoTracking()
                    .Where(x => GroupIDs.Contains(x.GroupID))
                    .OrderBy(x => x.GroupID).ThenBy(x => x.Brand)
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Items
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = Ex.Message,
                    Data = (object?)null
                });
            }
        }
    }
}
