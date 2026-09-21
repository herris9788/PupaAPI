using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Beesuite-owned, one-row-per-vessel table for vessel attributes that
    /// shouldn't live on "Ascend"."IC_InventoryUsers" or
    /// "Ascend"."InventoryUserSpecs" -- both are truncated and re-synced from
    /// the legacy Ascend system on a schedule (see BeesuiteGO's
    /// sync_staging_service.go usersTruncateOrder), so anything written there
    /// is lost on the next sync.
    ///
    /// Keyed by InventoryUserID (the vessel's stable business key, not the
    /// Ascend-mirror's internal "ID") as a SOFT reference -- deliberately no
    /// FK constraint to InventoryUser, so this row is never pulled into that
    /// table's TRUNCATE CASCADE.
    ///
    /// Shared table: also read/written by BeesuiteGO
    /// (models/vessel_detail.go) against the same Postgres "beesuite"
    /// database.
    /// </summary>
    [Table("VesselDetail")]
    public class VesselDetail : BaseEntity
    {
        [Key]
        [Column("InventoryUserID")]
        public virtual int InventoryUserID
        {
            get; set;
        }

        private bool _IsClass;
        [Column("IsClass")]
        public virtual bool IsClass
        {
            get => _IsClass;
            set { OnPropertyChanging(); _IsClass = value; OnPropertyChanged(); }
        }

        private string? _UpdatedBy;
        [Column("UpdatedBy")]
        public virtual string? UpdatedBy
        {
            get => _UpdatedBy;
            set { OnPropertyChanging(); _UpdatedBy = value; OnPropertyChanged(); }
        }

        private DateTime? _UpdatedAt;
        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get => _UpdatedAt;
            set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); }
        }
    }
}
