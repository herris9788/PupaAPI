using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
        // Returns an IQueryable filtered to the key (via SingleResult) — NOT a
        // materialized Find() — so [EnableQuery] can apply $select/$expand against
        // the database. Returning a single materialized entity would ignore $expand
        // (navigations would come back empty since lazy loading is disabled).
        [EnableQuery(MaxNodeCount = 500, MaxExpansionDepth = 500)]
        public SingleResult<TEntity> Get(int key)
        {
            return SingleResult.Create(FilterByKey(key));
        }

        // Builds "e => e.<PrimaryKey> == key" using the entity's actual primary-key
        // property name (Item -> ItemID, Requisition -> ID, etc.).
        private IQueryable<TEntity> FilterByKey(int key)
        {
            var keyProperty = _db.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[0];
            var keyName = keyProperty?.Name ?? "ID";

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(parameter, keyName);
            Expression keyConstant = Expression.Constant(key);
            if (property.Type != typeof(int))
            {
                keyConstant = Expression.Convert(keyConstant, property.Type);
            }
            var predicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(property, keyConstant), parameter);

            return _db.Set<TEntity>().Where(predicate);
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
                patch.Patch(entity);

                var patchUpdatesRevertStatus = patch.GetChangedPropertyNames()
                    .Any(propertyName => string.Equals(
                        propertyName,
                        nameof(Requisition.RevertStatus),
                        StringComparison.OrdinalIgnoreCase));

                if (entity is Requisition requisition &&
                    requisition.RevertStatus != null &&
                    !patchUpdatesRevertStatus)
                {
                    requisition.RevertStatus = null;
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
