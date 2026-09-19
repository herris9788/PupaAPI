using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;

namespace Pupa.Controllers
{
    [Route("beesuite/api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly BeesuiteDbContext _db;

        public MenuController(BeesuiteDbContext db)
        {
            _db = db;
        }

        // GET beesuite/api/Menu
        // Optional query params (all additive, default preserves prior behavior):
        //   allowMobile=true|false -> filter by Menu.AllowMobile
        //   includeInactive=true   -> skip the IsActive filter (for admin screens)
        //   codePrefix=NS_         -> filter MenuCode to a specific app's namespace
        //                             (e.g. "BS_" is BeeSuite's own catalog, "NS_" is
        //                             NetSuite's, keeping per-app admin screens from
        //                             mixing each other's rows in this shared table)
        //   category=QUICK_ACTION  -> exact-match filter on Menu.Category, so a client
        //                             can request just one grouping (e.g. NetSuite's
        //                             Home "Quick Access" row vs its Explore grid)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? allowMobile,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? codePrefix = null,
            [FromQuery] string? category = null)
        {
            try
            {
                var query = _db.Menu.AsNoTracking().AsQueryable();

                if (!includeInactive)
                    query = query.Where(m => m.IsActive);

                if (allowMobile.HasValue)
                    query = query.Where(m => m.AllowMobile == allowMobile.Value);

                if (!string.IsNullOrWhiteSpace(codePrefix))
                    query = query.Where(m => m.MenuCode != null && m.MenuCode.StartsWith(codePrefix));

                if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(m => m.Category == category);

                var data = await query
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.SortOrder)
                    .ToListAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET beesuite/api/Menu/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _db.Menu.AsNoTracking().FirstOrDefaultAsync(m => m.ID == id);
                if (item == null) return NotFound();
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // POST beesuite/api/Menu
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Menu dto)
        {
            try
            {
                _db.Menu.Add(dto);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = dto.ID }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT beesuite/api/Menu/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Menu dto)
        {
            try
            {
                var existing = await _db.Menu.FirstOrDefaultAsync(m => m.ID == id);
                if (existing == null) return NotFound();

                existing.MenuCode     = dto.MenuCode;
                existing.MenuName     = dto.MenuName;
                existing.Icon         = dto.Icon;
                existing.Route        = dto.Route;
                existing.Category     = dto.Category;
                existing.Description  = dto.Description;
                existing.SortOrder    = dto.SortOrder;
                existing.IsActive     = dto.IsActive;
                existing.IsComingSoon = dto.IsComingSoon;
                existing.AllowWeb     = dto.AllowWeb;
                existing.AllowMobile  = dto.AllowMobile;
                existing.HideAppBar   = dto.HideAppBar;
                existing.Headers      = dto.Headers;

                await _db.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE beesuite/api/Menu/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _db.Menu.FirstOrDefaultAsync(m => m.ID == id);
                if (existing == null) return NotFound();
                _db.Menu.Remove(existing);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
