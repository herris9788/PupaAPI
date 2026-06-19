using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Pupa.Configs
{
    /// <summary>
    /// Generic OData CRUD controller backed by EF Core (replacement for the
    /// DevExpress XAF Web API auto-generated data controllers). One closed
    /// generic instance is registered per entity type by
    /// <see cref="CustomGenericControllerFeatureProvider"/>, and renamed to the
    /// entity name by <see cref="GenericControllerNameConvention"/> so OData
    /// routing maps it to the matching entity set.
    /// </summary>
    public class CustomDataController<TEntity> : ODataController
            where TEntity : class
    {
        private readonly BeesuiteDbContext _db;

        public CustomDataController(BeesuiteDbContext db)
        {
            _db = db;
        }

        // GET /{EntitySet}
        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public IActionResult Get()
        {
            try
            {
                return Ok(_db.Set<TEntity>().AsQueryable());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /{EntitySet}(key)
        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public IActionResult Get(int key)
        {
            var entity = _db.Set<TEntity>().Find(key);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        // POST /{EntitySet}
        public async Task<IActionResult> Post([FromBody] TEntity value)
        {
            if (value == null)
            {
                return BadRequest("Incorrect body.");
            }
            try
            {
                _db.Set<TEntity>().Add(value);
                await _db.SaveChangesAsync();
                return Created(value);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PATCH /{EntitySet}(key)
        public async Task<IActionResult> Patch(int key, [FromBody] Delta<TEntity> patch)
        {
            if (patch == null)
            {
                return BadRequest("Incorrect body.");
            }
            var entity = await _db.Set<TEntity>().FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            try
            {
                var originalRequisitionStatus = entity is Requisition originalRequisition
                    ? NormalizeRequisitionStatus(originalRequisition.Status)
                    : string.Empty;

                patch.Patch(entity);

                if (entity is Requisition requisition)
                {
                    var currentStatus = NormalizeRequisitionStatus(requisition.Status);

                    if (currentStatus == "REJECTED")
                    {
                        if (string.IsNullOrWhiteSpace(requisition.RejectedBy))
                        {
                            return BadRequest("RejectedBy is required when Status is REJECTED.");
                        }

                        if (requisition.RejectedTime == null)
                        {
                            return BadRequest("RejectedTime is required when Status is REJECTED.");
                        }
                    }

                    if (originalRequisitionStatus == "REJECTED" && currentStatus == "PENDING")
                    {
                        requisition.RejectedBy = null;
                        requisition.RejectedTime = null;
                    }
                }

                await _db.SaveChangesAsync();
                return Updated(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT /{EntitySet}(key)
        public async Task<IActionResult> Put(int key, [FromBody] TEntity update)
        {
            if (update == null)
            {
                return BadRequest("Incorrect body.");
            }
            var entity = await _db.Set<TEntity>().FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            try
            {
                _db.Entry(entity).CurrentValues.SetValues(update);
                await _db.SaveChangesAsync();
                return Updated(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private static string NormalizeRequisitionStatus(string? status)
        {
            return string.IsNullOrWhiteSpace(status) ? string.Empty : status.Trim().ToUpperInvariant();
        }

        // DELETE /{EntitySet}(key)
        public async Task<IActionResult> Delete(int key)
        {
            var entity = await _db.Set<TEntity>().FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            try
            {
                _db.Set<TEntity>().Remove(entity);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
