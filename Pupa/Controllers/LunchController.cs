using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;

namespace Pupa.Controllers
{
    // NetSuite "Lunch" feature: an admin sets a reservation window
    // (OpenTime/CloseTime) and a list of dish options for the day (each
    // Halal or Non-Halal, with its own quota). Staff reserve one dish and
    // get a queue number; the admin confirms pickup at the counter.
    //
    // Reservation endpoints that act "as the signed-in user" require
    // [Authorize] — the mobile app's login JWT (issuer "hastalavista2021DD")
    // validates here too, so Identity.Name/NameIdentifier are trustworthy.
    // Admin/management endpoints intentionally have no [Authorize], matching
    // MenuController's pattern: the admin page is gated by X-App-Key, not a
    // per-user token.
    [Route("beesuite/api/[controller]")]
    [ApiController]
    public class LunchController : ControllerBase
    {
        private readonly BeesuiteDbContext _db;

        public LunchController(BeesuiteDbContext db)
        {
            _db = db;
        }

        private static DateTime NormalizeDate(DateTime? date) =>
            DateTime.SpecifyKind((date ?? DateTime.UtcNow).Date, DateTimeKind.Unspecified);

        private static bool IsValidChoice(string? choice) => choice == "Halal" || choice == "NonHalal";

        // Namespaces this feature's Postgres advisory locks so they never collide
        // with any other pg_advisory_xact_lock caller elsewhere in the codebase.
        private const int LunchLockNamespace = 0x4C554E43; // 'L','U','N','C' packed into an arbitrary stable int

        private static int DateLockKey(DateTime day) => day.Year * 10000 + day.Month * 100 + day.Day;

        // ---- Day settings (reservation window) ----
        //
        // The reservation window (open/close time + active toggle) is a single
        // standing setting, not something admins re-enter every day — it's
        // stored under this fixed sentinel "date" so one save applies to every
        // day going forward. `date` query/body params below are accepted for
        // backward compatibility with existing callers but are otherwise unused.
        private static readonly DateTime GlobalWindowDate = new DateTime(1900, 1, 1);

        // GET beesuite/api/Lunch/menu
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu([FromQuery] DateTime? date)
        {
            try
            {
                var menu = await _db.LunchMenu.AsNoTracking().FirstOrDefaultAsync(m => m.Date == GlobalWindowDate);
                if (menu == null) return NotFound();
                return Ok(menu);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        public class MenuSettingsRequest
        {
            public DateTime? Date { get; set; }
            public TimeOnly? OpenTime { get; set; }
            public TimeOnly? CloseTime { get; set; }
            public bool IsActive { get; set; } = true;
        }

        // POST beesuite/api/Lunch/menu  (admin: set the standing reservation window)
        [HttpPost("menu")]
        public async Task<IActionResult> SetMenu([FromBody] MenuSettingsRequest req)
        {
            try
            {
                var existing = await _db.LunchMenu.FirstOrDefaultAsync(m => m.Date == GlobalWindowDate);
                if (existing == null)
                {
                    existing = new LunchMenu { Date = GlobalWindowDate, OpenTime = req.OpenTime, CloseTime = req.CloseTime, IsActive = req.IsActive };
                    _db.LunchMenu.Add(existing);
                }
                else
                {
                    existing.OpenTime = req.OpenTime;
                    existing.CloseTime = req.CloseTime;
                    existing.IsActive = req.IsActive;
                }
                await _db.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ---- Menu items (dishes) ----

        public class MenuItemDto
        {
            public int ID { get; set; }
            public DateTime Date { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string Choice { get; set; } = "Halal";
            public int Quota { get; set; }
            public int ReservedCount { get; set; }
            public int? RemainingQuota { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; }
        }

        // GET beesuite/api/Lunch/menu-items?date=2026-09-12&includeInactive=true
        [HttpGet("menu-items")]
        public async Task<IActionResult> GetMenuItems([FromQuery] DateTime? date, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var day = NormalizeDate(date);
                var query = _db.LunchMenuItem.AsNoTracking().Where(m => m.Date == day);
                if (!includeInactive) query = query.Where(m => m.IsActive);

                var items = await query.OrderBy(m => m.Choice).ThenBy(m => m.SortOrder).ToListAsync();

                var counts = await _db.LunchReservation
                    .Where(r => r.Date == day && r.Status != "Cancelled" && r.MenuItemID != null)
                    .GroupBy(r => r.MenuItemID)
                    .Select(g => new { MenuItemID = g.Key!.Value, Count = g.Count() })
                    .ToDictionaryAsync(x => x.MenuItemID, x => x.Count);

                var result = items.Select(m =>
                {
                    var reserved = counts.TryGetValue(m.ID, out var c) ? c : 0;
                    return new MenuItemDto
                    {
                        ID = m.ID,
                        Date = m.Date,
                        Name = m.Name,
                        Description = m.Description,
                        Choice = m.Choice,
                        Quota = m.Quota,
                        SortOrder = m.SortOrder,
                        IsActive = m.IsActive,
                        ReservedCount = reserved,
                        RemainingQuota = m.Quota > 0 ? Math.Max(0, m.Quota - reserved) : null,
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        public class MenuItemRequest
        {
            public DateTime? Date { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string Choice { get; set; } = "Halal";
            public int Quota { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; } = true;
        }

        // POST beesuite/api/Lunch/menu-items  (admin: add a dish for a day)
        [HttpPost("menu-items")]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItemRequest req)
        {
            try
            {
                if (!IsValidChoice(req.Choice)) return BadRequest("Choice must be \"Halal\" or \"NonHalal\".");

                var item = new LunchMenuItem
                {
                    Date = NormalizeDate(req.Date),
                    Name = req.Name,
                    Description = req.Description,
                    Choice = req.Choice,
                    Quota = req.Quota,
                    SortOrder = req.SortOrder,
                    IsActive = req.IsActive,
                };
                _db.LunchMenuItem.Add(item);
                await _db.SaveChangesAsync();
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // PUT beesuite/api/Lunch/menu-items/5  (admin: edit a dish)
        [HttpPut("menu-items/{id:int}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItemRequest req)
        {
            try
            {
                if (!IsValidChoice(req.Choice)) return BadRequest("Choice must be \"Halal\" or \"NonHalal\".");

                var existing = await _db.LunchMenuItem.FirstOrDefaultAsync(m => m.ID == id);
                if (existing == null) return NotFound();

                existing.Name = req.Name;
                existing.Description = req.Description;
                existing.Choice = req.Choice;
                existing.Quota = req.Quota;
                existing.SortOrder = req.SortOrder;
                existing.IsActive = req.IsActive;

                await _db.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // DELETE beesuite/api/Lunch/menu-items/5  (admin: remove a dish)
        [HttpDelete("menu-items/{id:int}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var existing = await _db.LunchMenuItem.FirstOrDefaultAsync(m => m.ID == id);
                if (existing == null) return NotFound();
                _db.LunchMenuItem.Remove(existing);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ---- Reservations ----

        // GET beesuite/api/Lunch/reservations?date=2026-09-12  (admin: full list)
        [HttpGet("reservations")]
        public async Task<IActionResult> GetReservations([FromQuery] DateTime? date)
        {
            try
            {
                var day = NormalizeDate(date);
                var data = await _db.LunchReservation
                    .AsNoTracking()
                    .Where(r => r.Date == day && r.Status != "Cancelled")
                    .OrderBy(r => r.QueueNumber)
                    .ToListAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // GET beesuite/api/Lunch/reservations/me?date=2026-09-12  (staff: my reservation today)
        [Authorize]
        [HttpGet("reservations/me")]
        public async Task<IActionResult> GetMyReservation([FromQuery] DateTime? date)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var day = NormalizeDate(date);
                var reservation = await _db.LunchReservation
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Date == day && r.UserID == userId && r.Status != "Cancelled");
                if (reservation == null) return NotFound();
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        public class ReserveRequest
        {
            public int MenuItemID { get; set; }
        }

        // POST beesuite/api/Lunch/reservations  (staff: reserve a specific dish for myself)
        [Authorize]
        [HttpPost("reservations")]
        public async Task<IActionResult> Reserve([FromBody] ReserveRequest req)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var item = await _db.LunchMenuItem.FirstOrDefaultAsync(m => m.ID == req.MenuItemID);
                if (item == null || !item.IsActive) return NotFound("Menu tidak ditemukan.");

                var day = item.Date;

                // Idempotent: already reserved today (any dish) → return it as-is,
                // regardless of the window/quota checks below.
                var existing = await _db.LunchReservation
                    .FirstOrDefaultAsync(r => r.Date == day && r.UserID == userId && r.Status != "Cancelled");
                if (existing != null) return Ok(existing);

                var windowSettings = await _db.LunchMenu.AsNoTracking().FirstOrDefaultAsync(m => m.Date == GlobalWindowDate);
                var nowTime = TimeOnly.FromDateTime(DateTime.Now);
                if (windowSettings != null)
                {
                    if (!windowSettings.IsActive)
                        return StatusCode(403, "Reservasi lunch sedang dinonaktifkan.");
                    if (windowSettings.OpenTime.HasValue && nowTime < windowSettings.OpenTime.Value)
                        return StatusCode(403, $"Reservasi baru dibuka jam {windowSettings.OpenTime.Value:HH\\:mm}.");
                    if (windowSettings.CloseTime.HasValue && nowTime > windowSettings.CloseTime.Value)
                        return StatusCode(403, $"Reservasi sudah ditutup jam {windowSettings.CloseTime.Value:HH\\:mm}.");
                }

                // Serialize with every other reservation attempt for this same day so
                // the quota check and QueueNumber assignment below are atomic —
                // without this, two concurrent requests can both pass the quota
                // check (oversold by one), or both compute the same next
                // QueueNumber (plain read-then-write race; QueueNumber has no
                // unique DB constraint to catch it).
                await using var tx = await _db.Database.BeginTransactionAsync();
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock({LunchLockNamespace}, {DateLockKey(day)})");

                if (item.Quota > 0)
                {
                    var reservedCount = await _db.LunchReservation
                        .CountAsync(r => r.MenuItemID == item.ID && r.Status != "Cancelled");
                    if (reservedCount >= item.Quota) return Conflict("Kuota menu ini sudah habis.");
                }

                var nextQueue = 1 + await _db.LunchReservation
                    .Where(r => r.Date == day)
                    .Select(r => (int?)r.QueueNumber)
                    .MaxAsync() ?? 1;

                var reservation = new LunchReservation
                {
                    Date = day,
                    UserID = userId,
                    Username = username,
                    MenuItemID = item.ID,
                    MenuItemName = item.Name,
                    Choice = item.Choice,
                    QueueNumber = nextQueue,
                    Status = "Reserved",
                    CreatedAt = DateTime.UtcNow,
                    QrToken = Guid.NewGuid().ToString("N"),
                };
                _db.LunchReservation.Add(reservation);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        public class ManualReserveRequest
        {
            public string Username { get; set; } = "";
            public int MenuItemID { get; set; }
        }

        // POST beesuite/api/Lunch/reservations/manual  (admin: reserve on someone's
        // behalf — bypasses the reservation window and quota, same as walking up
        // to the counter and being handled directly)
        [HttpPost("reservations/manual")]
        public async Task<IActionResult> ManualReserve([FromBody] ManualReserveRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Username)) return BadRequest("Username is required.");

                var item = await _db.LunchMenuItem.FirstOrDefaultAsync(m => m.ID == req.MenuItemID);
                if (item == null) return NotFound("Menu tidak ditemukan.");

                var day = item.Date;

                // Same per-day serialization as Reserve() above — closes the
                // QueueNumber race between concurrent manual/self reservations.
                await using var tx = await _db.Database.BeginTransactionAsync();
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock({LunchLockNamespace}, {DateLockKey(day)})");

                var nextQueue = 1 + await _db.LunchReservation
                    .Where(r => r.Date == day)
                    .Select(r => (int?)r.QueueNumber)
                    .MaxAsync() ?? 1;

                var reservation = new LunchReservation
                {
                    Date = day,
                    UserID = null,
                    Username = req.Username.Trim(),
                    MenuItemID = item.ID,
                    MenuItemName = item.Name,
                    Choice = item.Choice,
                    QueueNumber = nextQueue,
                    Status = "Reserved",
                    CreatedAt = DateTime.UtcNow,
                    QrToken = Guid.NewGuid().ToString("N"),
                };
                _db.LunchReservation.Add(reservation);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        public class ScanRequest
        {
            public string Token { get; set; } = "";
        }

        public class ScanResultDto
        {
            public int ID { get; set; }
            public int QueueNumber { get; set; }
            public string? Username { get; set; }
            public string? MenuItemName { get; set; }
            public string? Choice { get; set; }
            public string Status { get; set; } = "";
            public bool AlreadyPickedUp { get; set; }
        }

        // POST beesuite/api/Lunch/reservations/scan  (admin: confirm pickup by
        // scanning the QR code shown on the staff member's ticket — same
        // effect as Confirm(id) below, just looked up by QrToken instead of a
        // row the admin picked out of the list by hand)
        [HttpPost("reservations/scan")]
        public async Task<IActionResult> Scan([FromBody] ScanRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Token)) return BadRequest("Token is required.");

                var reservation = await _db.LunchReservation.FirstOrDefaultAsync(r => r.QrToken == req.Token);
                if (reservation == null) return NotFound("QR tidak dikenali / reservasi tidak ditemukan.");
                if (reservation.Status == "Cancelled") return Conflict("Reservasi ini sudah dibatalkan.");

                var alreadyPickedUp = reservation.Status == "PickedUp";
                if (!alreadyPickedUp)
                {
                    reservation.Status = "PickedUp";
                    reservation.PickedUpAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }

                return Ok(new ScanResultDto
                {
                    ID = reservation.ID,
                    QueueNumber = reservation.QueueNumber,
                    Username = reservation.Username,
                    MenuItemName = reservation.MenuItemName,
                    Choice = reservation.Choice,
                    Status = reservation.Status,
                    AlreadyPickedUp = alreadyPickedUp,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // POST beesuite/api/Lunch/reservations/5/confirm  (admin: mark picked up)
        [HttpPost("reservations/{id:int}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var reservation = await _db.LunchReservation.FirstOrDefaultAsync(r => r.ID == id);
                if (reservation == null) return NotFound();
                reservation.Status = "PickedUp";
                reservation.PickedUpAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // POST beesuite/api/Lunch/reservations/5/undo-pickup  (admin: revert a
        // mistaken pickup confirmation back to "Reserved" — e.g. the wrong
        // ticket was scanned/confirmed by accident)
        [HttpPost("reservations/{id:int}/undo-pickup")]
        public async Task<IActionResult> UndoPickup(int id)
        {
            try
            {
                var reservation = await _db.LunchReservation.FirstOrDefaultAsync(r => r.ID == id);
                if (reservation == null) return NotFound();
                if (reservation.Status != "PickedUp") return BadRequest("Reservasi ini belum diambil.");
                reservation.Status = "Reserved";
                reservation.PickedUpAt = null;
                await _db.SaveChangesAsync();
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // DELETE beesuite/api/Lunch/reservations/5  (admin: cancel a reservation —
        // frees up both the day's queue and that dish's quota)
        [HttpDelete("reservations/{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var reservation = await _db.LunchReservation.FirstOrDefaultAsync(r => r.ID == id);
                if (reservation == null) return NotFound();
                reservation.Status = "Cancelled";
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
