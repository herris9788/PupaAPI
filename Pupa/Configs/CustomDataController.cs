using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq.Expressions;
using System.Reflection;

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

                if (value is UserApprovalScope2 v2Created && v2Created.VesselID != null)
                {
                    await SyncLegacyMirrorForVessel(v2Created.VesselID.Value, v2Created.Level);
                    await _db.SaveChangesAsync();
                }

                return Created(value);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PATCH /{EntitySet}(key)
        public async Task<IActionResult> Patch(int key, [FromBody] JsonElement patch)
        {
            if (patch.ValueKind != JsonValueKind.Object)
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
                // Captured BEFORE the patch so a PATCH that moves a
                // UserApprovalScope2 row to/from a different VesselID or
                // Level re-syncs BOTH the old and new vessel+level's mirror
                // (see SyncLegacyMirrorForVessel below).
                int? preVesselId = (entity as UserApprovalScope2)?.VesselID;
                short? preLevel = (entity as UserApprovalScope2)?.Level;

                var changedPropertyNames = ApplyPatch(entity, patch);

                var patchUpdatesRevertStatus = changedPropertyNames
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

                if (entity is UserApprovalScope2 v2Patched)
                {
                    if (preVesselId != null)
                    {
                        await SyncLegacyMirrorForVessel(preVesselId.Value, preLevel);
                    }
                    if (v2Patched.VesselID != null &&
                        (v2Patched.VesselID != preVesselId || v2Patched.Level != preLevel))
                    {
                        await SyncLegacyMirrorForVessel(v2Patched.VesselID.Value, v2Patched.Level);
                    }
                    await _db.SaveChangesAsync();
                }

                return Updated(entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private static HashSet<string> ApplyPatch(TEntity entity, JsonElement patch)
        {
            var changedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var jsonProperty in patch.EnumerateObject())
            {
                if (jsonProperty.Name.StartsWith("@", StringComparison.Ordinal))
                {
                    continue;
                }

                var clrProperty = typeof(TEntity).GetProperty(
                    jsonProperty.Name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (clrProperty == null || !clrProperty.CanWrite)
                {
                    continue;
                }

                var propertyType = clrProperty.PropertyType;
                object? value;

                if (jsonProperty.Value.ValueKind == JsonValueKind.Null)
                {
                    if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null)
                    {
                        throw new InvalidOperationException($"Property '{clrProperty.Name}' cannot be null.");
                    }

                    value = null;
                }
                else
                {
                    value = JsonSerializer.Deserialize(
                        jsonProperty.Value.GetRawText(),
                        propertyType,
                        jsonOptions);

                    // DateTime columns in this DB store server-local (WIB) wall-clock
                    // values with no offset (e.g. CreatedAt via DateTime.Now). Incoming
                    // ISO-8601 strings with a "Z"/offset deserialize as Kind=Utc, so they
                    // must be converted to local time here to match that convention -
                    // otherwise the stored value ends up ~7h off (see ApprovedByNAt bug).
                    if (value is DateTime dateTimeValue && dateTimeValue.Kind == DateTimeKind.Utc)
                    {
                        value = dateTimeValue.ToLocalTime();
                    }
                }

                clrProperty.SetValue(entity, value);
                changedPropertyNames.Add(clrProperty.Name);
            }

            return changedPropertyNames;
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
                // Captured before removal — see SyncLegacyMirrorForVessel.
                int? deletedVesselId = (entity as UserApprovalScope2)?.VesselID;
                short? deletedLevel = (entity as UserApprovalScope2)?.Level;

                _db.Set<TEntity>().Remove(entity);
                await _db.SaveChangesAsync();

                if (deletedVesselId != null)
                {
                    await SyncLegacyMirrorForVessel(deletedVesselId.Value, deletedLevel);
                    await _db.SaveChangesAsync();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Keeps a legacy UserApprovalScope (v1) row in sync with a
        /// full-wildcard UserApprovalScope2 (v2) row scoped to one specific
        /// vessel — so the ALREADY-PUBLISHED mobile app (whose Job Request
        /// approver lookup only ever reads UserApprovalScope, hardcoding
        /// StockCategoryID/StockFamilyID to -1, and never reads
        /// UserApprovalScope2 at all) picks up a v2 "All Categories, All
        /// Families" rule without needing an app update/store release.
        ///
        /// Deliberately narrow scope:
        ///  - Only mirrors when the v2 row is a full wildcard for stock
        ///    (StockCategoryID AND StockFamilyID both null). A
        ///    category-specific v2 rule can never be represented this way —
        ///    the old mobile code discards any non-(-1) row for Job
        ///    Requests by design (hardcoded client logic); no backend/data
        ///    trick can change that without a new app build.
        ///  - Only mirrors when the v2 row targets one specific VesselID
        ///    (not a whole VesselGroupID) — a group-wide v2 rule would need
        ///    one v1 row per CompanyDB actually used in that group, which
        ///    this method doesn't attempt.
        /// Re-derives (rather than diffs) the mirror row for
        /// [vesselId]/[level] from whatever currently-active v2 row best
        /// matches, so edits/deletes/reactivations on the v2 side stay
        /// correctly reflected without separate add/remove bookkeeping.
        ///
        /// KNOWN SIDE EFFECT: because UserApprovalScope's StockCategoryID/
        /// StockFamilyID = -1 is a wildcard for EVERY consumer of that table
        /// (not just Job Requests), this mirror row also becomes a
        /// fallback approver for Item Requisitions on this vessel+level if
        /// the primary backend resolver (/User/Approver) is ever
        /// unreachable and the old mobile app falls back to local
        /// UserApprovalScope matching. Acceptable for now — vessels here
        /// are ApprovalRuleVersion 2 already, so that fallback path is not
        /// the normal one — but worth knowing before mirroring is applied
        /// more broadly.
        /// </summary>
        private async Task SyncLegacyMirrorForVessel(int vesselId, short? level)
        {
            if (level == null) return;

            // .Include(Group) is required here: InventoryUser.GroupID is a
            // legacy scalar column that is NOT the referenced group's PK (it
            // repeats across multiple real InventoryUserGroup rows in
            // different companies — e.g. GroupID=6 exists on 4 different
            // groups). Every matcher (ResolveApprovers, PendingApproval,
            // MatchesV2) compares UserApprovalScope(2).VesselGroupID against
            // Vessel.Group.ID — the navigated PK — not Vessel.GroupID.
            // Confirmed against real data: MV. AMETHYST has GroupID=6 but its
            // actual InventoryUserGroup.ID is 50.
            var vessel = await _db.InventoryUser.AsNoTracking()
                .Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.ID == vesselId);
            if (vessel?.Group == null || string.IsNullOrEmpty(vessel.DB))
            {
                // Can't resolve what the old mobile query needs (VesselGroupID +
                // CompanyDB) — nothing safe to mirror.
                return;
            }

            // Most-specific currently-active v2 row that is STILL a full
            // wildcard for this exact vessel+level (mirrors the resolver's own
            // Specificity ordering, so this stays correct even if a
            // more-specific non-wildcard row is later added at the same level —
            // that row would win in ResolveApprovers, and should stop being
            // mirrored here too).
            var stillWildcard = await _db.UserApprovalScope2.AsNoTracking()
                .Where(s => s.IsActive != false && s.Level == level && s.VesselID == vesselId
                         && s.StockCategoryID == null && s.StockFamilyID == null)
                .OrderByDescending(s => s.Specificity).ThenBy(s => s.ID)
                .FirstOrDefaultAsync();

            // NOTE: the old mobile app's local match compares this column
            // against JobRequest.VesselInventoryUserRowID directly — which,
            // despite the name, holds InventoryUser.ID (the PK, == vesselId
            // here), NOT InventoryUser.InventoryUserID (a separate, unrelated
            // legacy column). Confirmed against real data before writing this.
            var mirrorRow = await _db.UserApprovalScope
                .FirstOrDefaultAsync(r => r.InventoryUserID == vesselId
                                       && r.VesselGroupID == vessel.Group.ID
                                       && r.CompanyDB == vessel.DB
                                       && r.StockCategoryID == -1 && r.StockFamilyID == -1
                                       && r.Level == level
                                       && r.Department == null && r.SubDepartment == null);

            if (stillWildcard?.UserID == null)
            {
                if (mirrorRow != null) _db.UserApprovalScope.Remove(mirrorRow);
                return;
            }

            if (mirrorRow == null)
            {
                _db.UserApprovalScope.Add(new UserApprovalScope
                {
                    UserID = stillWildcard.UserID,
                    CompanyDB = vessel.DB,
                    VesselGroupID = vessel.Group.ID,
                    InventoryUserID = vesselId,
                    StockCategoryID = -1,
                    StockFamilyID = -1,
                    Level = level,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });
            }
            else if (mirrorRow.UserID != stillWildcard.UserID)
            {
                mirrorRow.UserID = stillWildcard.UserID;
                mirrorRow.UpdatedAt = DateTime.Now;
            }
        }
    }
}
