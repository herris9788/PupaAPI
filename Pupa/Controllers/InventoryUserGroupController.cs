using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Pupa.Controllers
{
    public class InventoryUserGroupController : ODataController
    {
        private readonly BeesuiteDbContext _db;

        public InventoryUserGroupController(BeesuiteDbContext db)
        {
            _db = db;
        }

        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public IActionResult Get()
        {
            return Ok(_db.InventoryUserGroup.AsQueryable());
        }

        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public IActionResult Get([FromODataUri] int key)
        {
            var obj = _db.InventoryUserGroup.Find(key);
            if (obj == null)
            {
                return NotFound();
            }
            return Ok(obj);
        }

        public async Task<IActionResult> Post([FromBody] InventoryUserGroup value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newObj = new InventoryUserGroup
            {
                DB = value.DB,
                GroupID = value.GroupID,
                GroupCode = value.GroupCode,
                GroupName = value.GroupName
            };

            _db.InventoryUserGroup.Add(newObj);
            await _db.SaveChangesAsync();
            return Created(newObj);
        }

        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<InventoryUserGroup> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var obj = await _db.InventoryUserGroup.FindAsync(key);
            if (obj == null)
            {
                return NotFound();
            }

            patch.Patch(obj);
            await _db.SaveChangesAsync();
            return Updated(obj);
        }

        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] InventoryUserGroup update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var obj = await _db.InventoryUserGroup.FindAsync(key);
            if (obj == null)
            {
                return NotFound();
            }

            obj.GroupCode = update.GroupCode;
            obj.GroupName = update.GroupName;

            await _db.SaveChangesAsync();
            return Updated(obj);
        }

        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var obj = await _db.InventoryUserGroup.FindAsync(key);
            if (obj == null)
            {
                return NotFound();
            }
            _db.InventoryUserGroup.Remove(obj);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
