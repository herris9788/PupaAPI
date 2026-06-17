using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Security.Policy;
using System.Drawing;
using System.Threading;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects.Beesuite;
using Pupa.BusinessObjects;


namespace Pupa.Controllers
{

    [Route("beesuite/api/[controller]")]
    public class EventController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly BeesuiteDbContext _db;
        public EventController(BeesuiteDbContext db)
        {
            _db = db;
        }
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] Event Body)
        {
            if (Body == null) return BadRequest("Data kosong");

            try
            {
                if (Body.ID <= 0)
                {
                    // --- LOGIC CREATE ---
                    Body.CreatedAt = DateTime.Now;

                    // EF Core akan otomatis insert ke tabel child jika list tidak kosong
                    _db.Set<Event>().Add(Body);
                }
                else
                {
                    // --- LOGIC UPDATE ---
                    // Gunakan .Include agar child data ikut terambil untuk sinkronisasi
                    var existing = await _db.Set<Event>()
                        .Include(x => x.Items)
                        .Include(x => x.Participants)
                        .Include(x => x.SpecificationItems)
                        .FirstOrDefaultAsync(x => x.ID == Body.ID);

                    if (existing == null) return NotFound("Data tidak ditemukan");

                    // 1. Update Parent Properties
                    existing.Title = Body.Title;
                    existing.Description = Body.Description;
                    existing.Date = Body.Date;
                    existing.Type = Body.Type;
                    existing.UpdatedAt = DateTime.Now;

                    var existingItemIDs = existing.Items.Select(x => x.ID).ToList();
                    var bodItemIDs = Body.Items.Select(x => x.ID).ToList();
                    var newItems = Body.Items.Where(x => !existingItemIDs.Contains(x.ID)).ToList();
                    var updateItems = Body.Items.Where(x => existingItemIDs.Contains(x.ID)).ToList();
                    var deletedItems = existing.Items.Where(x => !bodItemIDs.Contains(x.ID)).ToList();

                    if(newItems.Count() > 0)
                    {
                        foreach(var i in newItems)
                        {
                            existing.Items.Add(i);
                            _db.Add(i);
                            _db.Entry(i).State = EntityState.Added;
                        }
                    }
                    if (updateItems.Count() > 0)
                    {
                        foreach (var i in updateItems)
                        {
                            _db.Entry(i).State = EntityState.Modified;
                        }
                    }
                    if (deletedItems.Count() > 0)
                    {
                        foreach (var i in deletedItems)
                        {
                            existing.Items.Remove(i);
                            _db.Remove(i);
                            _db.Entry(i).State = EntityState.Deleted;
                        }
                    }


                    //// 2. Update Child: Items (Hapus yang lama, ganti yang baru)
                    //if (existing.Items != null) _db.Set<EventHamperItem>().RemoveRange(existing.Items);
                    //existing.Items = Body.Items;

                    //// 3. Update Child: Participants
                    //if (existing.Participants != null) _db.Set<EventParticipant>().RemoveRange(existing.Participants);
                    //existing.Participants = Body.Participants;

                    //// 4. Update Child: UserSpecificItems
                    //if (existing.SpecificationItems != null) _db.Set<EventUserSpecificItem>().RemoveRange(existing.SpecificationItems);
                    //existing.SpecificationItems = Body.SpecificationItems;

                    _db.Set<Event>().Update(existing);
                }

                await _db.SaveChangesAsync();
                return Ok(Body);
            }
            catch (Exception ex)
            {
                // Jika ada error foreign key atau lainnya
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Endpoint Delete
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _db.Set<Event>().FindAsync(id);
            if (data == null) return NotFound();

            _db.Set<Event>().Remove(data);
            await _db.SaveChangesAsync();
            return Ok(true);
        }

    }
}
