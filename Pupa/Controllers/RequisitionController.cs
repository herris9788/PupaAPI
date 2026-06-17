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
using System.Collections.ObjectModel;


namespace Pupa.Controllers
{

    [Route("beesuite/api/[controller]")]
    public class RequisitionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly BeesuiteDbContext _db;
        public RequisitionController(BeesuiteDbContext db)
        {
            _db = db;
        }


        [HttpPost("Create")]
        public async Task<IActionResult> CreateRequisition([FromBody] Requisition Body)
        {
            try
            {
                if (Body == null)
                    return BadRequest("Request body cannot be null.");
                var vessel = await _db.InventoryUser.FirstOrDefaultAsync(x => x.ID == Body.VesselID);
                if (vessel == null)
                    return NotFound($"Vessel with ID {Body.VesselID} not found.");
                var docFormat = await _db.DocumentNumbering.FirstOrDefaultAsync(x => x.Vessel == vessel.InventoryUserName && x.Type == "ItemRequest");
                if (docFormat == null)
                    return NotFound($"Document numbering format for vessel '{vessel.InventoryUserName}' not found.");
                var now = DateTime.Now;
                var yearStr = now.ToString("yy");
                var monthStr = now.ToString("MM");
                var prefix = docFormat.Format
                    .Replace("<VC>", docFormat.VesselCode)
                    .Replace("<YY>", yearStr)
                    .Replace("<MM>", monthStr)
                    .Replace("<N4>", "");
                var lastNumber = await _db.Requisition
                    .Where(x => x.RequisitionNumber.StartsWith(prefix) &&
                                x.CreatedAt.Value.Year == now.Year &&
                                x.CreatedAt.Value.Month == now.Month)
                    .OrderByDescending(x => x.RequisitionNumber)
                    .Select(x => x.RequisitionNumber)
                    .FirstOrDefaultAsync();
                int nextNumber = 1;
                if (lastNumber != null)
                {
                    var lastSeq = lastNumber.Substring(prefix.Length);
                    if (int.TryParse(lastSeq, out int parsed))
                        nextNumber = parsed + 1;
                }
                Body.RequisitionNumber = docFormat.Format
                    .Replace("<VC>", docFormat.VesselCode)
                    .Replace("<YY>", yearStr)
                    .Replace("<MM>", monthStr)
                    .Replace("<N4>", nextNumber.ToString("D4"));
                Body.CreatedAt = now;
                await _db.AddAsync(Body);
                await _db.SaveChangesAsync();
                Body.RequisitionDetails = new ObservableCollection<RequisitionDetail>();
                Body.InventoryUser = null;
                return Ok(Body);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    Message = "Database error occurred while saving requisition.",
                    Detail = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred.",
                    Detail = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

    }
}
