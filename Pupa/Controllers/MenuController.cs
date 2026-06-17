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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _db.Menu
                    .AsNoTracking()
                    .Where(m => m.IsActive)
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

                existing.MenuCode    = dto.MenuCode;
                existing.MenuName    = dto.MenuName;
                existing.Icon        = dto.Icon;
                existing.Route       = dto.Route;
                existing.Category    = dto.Category;
                existing.Description = dto.Description;
                existing.SortOrder   = dto.SortOrder;
                existing.IsActive    = dto.IsActive;

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
