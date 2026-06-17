using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;

namespace Pupa.Controllers
{
    [Route("beesuite/api/[controller]")]
    [ApiController]
    public class LaunchPointController : ControllerBase
    {
        private readonly BeesuiteDbContext _db;

        public LaunchPointController(BeesuiteDbContext db)
        {
            _db = db;
        }

        public class LaunchPointTreeDto
        {
            public int ID { get; set; }
            public string? UserName { get; set; }
            public int MenuID { get; set; }
            public string? MenuCode { get; set; }
            public string? MenuName { get; set; }
            public string? Icon { get; set; }
            public string? Route { get; set; }
            public string? Category { get; set; }
            public int? ParentID { get; set; }
            public int SortOrder { get; set; }
            public List<LaunchPointTreeDto> Children { get; set; } = [];
        }

       

        // POST beesuite/api/LaunchPoint
        // Drag menu ke launchpoint user
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LaunchPoint dto)
        {
            try
            {
                _db.LaunchPoint.Add(dto);
                await _db.SaveChangesAsync();
                return Ok(new { dto.ID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT beesuite/api/LaunchPoint/5
        // Update posisi / parent (pindah node di treeview)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] LaunchPoint dto)
        {
            try
            {
                var existing = await _db.LaunchPoint.FirstOrDefaultAsync(lp => lp.ID == id);
                if (existing == null) return NotFound();

                existing.ParentID  = dto.ParentID;
                existing.SortOrder = dto.SortOrder;
                existing.IsActive  = dto.IsActive;

                await _db.SaveChangesAsync();
                return Ok(new { existing.ID, existing.ParentID, existing.SortOrder });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT beesuite/api/LaunchPoint/user/johndoe/reorder
        // Bulk update sort order setelah drag-drop
        [HttpPut("user/{userName}/reorder")]
        public async Task<IActionResult> Reorder(string userName, [FromBody] List<ReorderDto> items)
        {
            try
            {
                var ids = items.Select(i => i.ID).ToList();
                var existing = await _db.LaunchPoint
                    .Where(lp => lp.UserName == userName && ids.Contains(lp.ID))
                    .ToListAsync();

                foreach (var item in items)
                {
                    var lp = existing.FirstOrDefault(x => x.ID == item.ID);
                    if (lp == null) continue;
                    lp.ParentID  = item.ParentID;
                    lp.SortOrder = item.SortOrder;
                }

                await _db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE beesuite/api/LaunchPoint/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _db.LaunchPoint.FirstOrDefaultAsync(lp => lp.ID == id);
                if (existing == null) return NotFound();
                _db.LaunchPoint.Remove(existing);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE beesuite/api/LaunchPoint/user/johndoe
        // Hapus semua launchpoint milik user
        [HttpDelete("user/{userName}")]
        public async Task<IActionResult> DeleteByUser(string userName)
        {
            try
            {
                var items = await _db.LaunchPoint
                    .Where(lp => lp.UserName == userName)
                    .ToListAsync();
                _db.LaunchPoint.RemoveRange(items);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private static List<LaunchPointTreeDto> BuildTree(List<LaunchPointTreeDto> all, int? parentId)
        {
            return all
                .Where(x => x.ParentID == parentId)
                .OrderBy(x => x.SortOrder)
                .Select(x =>
                {
                    x.Children = BuildTree(all, x.ID);
                    return x;
                })
                .ToList();
        }

        public class ReorderDto
        {
            public int ID { get; set; }
            public int? ParentID { get; set; }
            public int SortOrder { get; set; }
        }
    }
}
