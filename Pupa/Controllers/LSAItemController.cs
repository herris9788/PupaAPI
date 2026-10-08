using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Pupa.Controllers
{
    /// <summary>
    /// LSAItem punya key gabungan (ItemID, SupplierID), sedangkan CustomDataController&lt;T&gt; hanya
    /// mengenal key tunggal -- jadi entity ini punya controller sendiri (seperti InventoryUserGroup).
    /// Akses per key: /LSAItem(ItemID=1,SupplierID=2)
    /// </summary>
    public class LSAItemController : ODataController
    {
        private readonly BeesuiteDbContext _db;

        public LSAItemController(BeesuiteDbContext db)
        {
            _db = db;
        }

        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public IActionResult Get()
        {
            return Ok(_db.LSAItem.AsQueryable());
        }

        [EnableQuery]
        public IActionResult Get([FromODataUri] int keyItemID, [FromODataUri] int keySupplierID)
        {
            return Ok(SingleResult.Create(_db.LSAItem.Where(x => x.ItemID == keyItemID && x.SupplierID == keySupplierID)));
        }

        public async Task<IActionResult> Post([FromBody] LSAItem value)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.LSAItem.Add(value);
            await _db.SaveChangesAsync();
            return Created(value);
        }

        public async Task<IActionResult> Patch([FromODataUri] int keyItemID, [FromODataUri] int keySupplierID, [FromBody] Delta<LSAItem> delta)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var obj = await _db.LSAItem.FirstOrDefaultAsync(x => x.ItemID == keyItemID && x.SupplierID == keySupplierID);
            if (obj == null) return NotFound();
            delta.Patch(obj);
            await _db.SaveChangesAsync();
            return Updated(obj);
        }

        public async Task<IActionResult> Delete([FromODataUri] int keyItemID, [FromODataUri] int keySupplierID)
        {
            var obj = await _db.LSAItem.FirstOrDefaultAsync(x => x.ItemID == keyItemID && x.SupplierID == keySupplierID);
            if (obj == null) return NotFound();
            _db.LSAItem.Remove(obj);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
