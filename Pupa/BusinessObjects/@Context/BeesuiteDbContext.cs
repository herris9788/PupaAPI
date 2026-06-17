using Pupa;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pupa.BusinessObjects
{
    //This factory creates DbContext for design-time services. For example, it is required for database migration.
    public class BeesuiteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<BeesuiteDbContext>
    {
        public BeesuiteDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            var conn = configuration.GetConnectionString("Beesuite");
            var optionsBuilder = new DbContextOptionsBuilder<BeesuiteDbContext>();
            optionsBuilder.UseNpgsql(conn);
            optionsBuilder.UseLazyLoadingProxies();
            return new BeesuiteDbContext(optionsBuilder.Options);
        }
    }

    public class BeesuiteDbContext : DbContext
    {
        public BeesuiteDbContext(DbContextOptions<BeesuiteDbContext> options) : base(options)
        {
        }
        public DbSet<Item> Item { get; set; }
        public DbSet<UOM> UOM { get; set; }
        public DbSet<ItemCategory> ItemCategory { get; set; }
        public DbSet<ServiceCategory> ServiceCategory { get; set; }
        public DbSet<Specification> Specification { get; set; }
        public DbSet<User> User { get;set; }
        public DbSet<UserVesselRel> UserVesselRel { get; set; }
        public DbSet<Requisition> Requisition { get; set; }
        public DbSet<Attachment> Attachment { get; set; }

        public DbSet<RequisitionDetail> RequisitionDetail { get; set; }
        public DbSet<VesselSpecRel> VesselSpecRel { get; set; }
        public DbSet<StockCategory> StockCategory { get; set; }
        public DbSet<StockFamily> StockFamily { get; set; }
        public DbSet<StockFamilyCOA> StockFamilyCOA { get; set; }
        public DbSet<InventoryUser> InventoryUser { get; set; }
        public DbSet<InventoryUserGroup> InventoryUserGroup { get; set; }
        public DbSet<Approval> Approval { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<EventHamperItem> EventHamperItem { get; set; }
        public DbSet<EventParticipant> EventParticipant { get; set; }
        public DbSet<EventUserSpecificItem> EventUserSpecificItem { get; set; }
        public DbSet<UserV2> UserV2 { get; set; }
        public DbSet<UserV3> UserV3 { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationRead> NotificationRead { get; set; }
        public DbSet<InventoryUserSpec> InventoryUserSpec { get; set; }
        public DbSet<DocumentNumbering> DocumentNumbering { get; set; }
        public DbSet<ROB> ROB { get; set; }
        public DbSet<PartBook> PartBook { get; set; }
        public DbSet<Job> Job { get; set; }
        public DbSet<JobRequest> JobRequest { get; set; }
        public DbSet<JobDetail> JobDetail { get; set; }
        public DbSet<UserApprovalScope> UserApprovalScope { get; set; }
        public DbSet<LogActivity> LogActivity { get; set; }
        public DbSet<RequisitionEngineNumber> RequisitionEngineNumber { get; set; }
        public DbSet<RequisitionCylinderNumber> RequisitionCylinderNumber { get; set; }
        public DbSet<RequisitionDetailAttachmentRel> RequisitionDetailAttachmentRel { get; set; }
        public DbSet<WhatsappDevice> WhatsappDevice { get; set; }
        public DbSet<WhatsappDeviceGroup> WhatsappDeviceGroup { get; set; }
        public DbSet<LaunchPoint> LaunchPoint { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<LaunchPointTemplate> LaunchPointTemplate { get; set; }
        public DbSet<UserPermission> UserPermission { get; set; }
        public DbSet<OrgDelegation> OrgDelegation { get; set; }
        public DbSet<OrgLevel> OrgLevel { get; set; }
        public DbSet<OrgPosition> OrgPosition { get; set; }
        public DbSet<OrgDepartment> OrgDepartment { get; set; }
        public DbSet<OrgPositionDepartment> OrgPositionDepartment { get; set; }
        public DbSet<TemplatePermission> TemplatePermission { get; set; }
        public DbSet<MenuFeature> MenuFeature { get; set; }
        public DbSet<ErrorLog> ErrorLog { get; set; }
        public DbSet<AccessLog> AccessLog { get; set; }
        public DbSet<AppConfig> AppConfig { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
                var conn = configuration.GetConnectionString("Beesuite");
                optionsBuilder.UseNpgsql(conn);
                optionsBuilder.UseLazyLoadingProxies();
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Replacement for DevExpress' SetOneToManyAssociationDeleteBehavior:
            // optional (nullable FK) associations -> SetNull, required -> Cascade.
            // The explicit configuration below overrides these defaults where needed.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var fk in entityType.GetForeignKeys())
                {
                    fk.DeleteBehavior = fk.IsRequired ? DeleteBehavior.Cascade : DeleteBehavior.SetNull;
                }
            }

            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

            // ── UserV3 collections — FK di masing-masing tabel mengarah ke User ─
            // Tanpa Ignore, EF Core generate shadow FK UserV3ID yang tidak ada di DB
            modelBuilder.Entity<UserV3>()
                .Ignore(u => u.UserApprovalScopes)
                .Ignore(u => u.UserVesselRels);

            modelBuilder.Entity<InventoryUser>().HasKey(iu => iu.ID);
            modelBuilder.Entity<InventoryUserGroup>().HasKey(iug => iug.ID);
            modelBuilder.Entity<InventoryUser>()
                .HasOne(iu => iu.Group)
                .WithMany(iug => iug.InventoryUsers)
                .HasForeignKey(iu => new { iu.DB, iu.GroupID })
                .HasPrincipalKey(iug => new { iug.DB, iug.GroupID });
            modelBuilder.Entity<Item>()
                .HasMany(x => x.ROBs)
                .WithOne(i => i.Item)
                .HasForeignKey("ItemCode")
                .HasPrincipalKey("ItemCode");
            modelBuilder.Entity<ROB>()
                .HasOne(x => x.InventoryUser)
                .WithMany(x => x.ROBs)
                  .HasForeignKey(iu => new { iu.DB, iu.InventoryUserCode })
                .HasPrincipalKey(iug => new { iug.DB, iug.InventoryUserCode });

            modelBuilder.Entity<JobFieldDefinition>()
               .HasIndex(u => new { u.FormType, u.FieldKey })
               .IsUnique();

            // ── OrgPosition self-reference ────────────────────────────────────
            modelBuilder.Entity<OrgPosition>()
                .HasOne(p => p.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(p => p.ParentPositionID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── OrgPosition → UserV3 (pakai UserID, bukan shadow UserV3ID) ────
            modelBuilder.Entity<OrgPosition>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ── OrgDepartment self-reference ──────────────────────────────────
            modelBuilder.Entity<OrgDepartment>()
                .HasOne(d => d.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(d => d.ParentID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ── OrgPositionDepartment (many-to-many junction) ─────────────────
            modelBuilder.Entity<OrgPositionDepartment>()
                .HasIndex(pd => new { pd.PositionID, pd.DepartmentID })
                .IsUnique();

            modelBuilder.Entity<OrgPositionDepartment>()
                .HasOne(pd => pd.Position)
                .WithMany(p => p.PositionDepartments)
                .HasForeignKey(pd => pd.PositionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrgPositionDepartment>()
                .HasOne(pd => pd.Department)
                .WithMany(d => d.PositionDepartments)
                .HasForeignKey(pd => pd.DepartmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // ── OrgPosition → OrgLevel ────────────────────────────────────────
            modelBuilder.Entity<OrgPosition>()
                .HasOne(p => p.Level)
                .WithMany(l => l.Positions)
                .HasForeignKey(p => p.LevelID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── OrgDelegation → OrgDepartment (dua FK berbeda) ───────────────
            modelBuilder.Entity<OrgDelegation>()
                .HasOne(d => d.FromDepartment)
                .WithMany(t => t.DelegationsFrom)
                .HasForeignKey(d => d.FromDepartmentID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrgDelegation>()
                .HasOne(d => d.ToDepartment)
                .WithMany(t => t.DelegationsTo)
                .HasForeignKey(d => d.ToDepartmentID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ── OrgDelegation → OrgPosition (dua FK berbeda) ─────────────────
            modelBuilder.Entity<OrgDelegation>()
                .HasOne(d => d.FromPosition)
                .WithMany(p => p.DelegationsFrom)
                .HasForeignKey(d => d.FromPositionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrgDelegation>()
                .HasOne(d => d.ToPosition)
                .WithMany(p => p.DelegationsTo)
                .HasForeignKey(d => d.ToPositionID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── LaunchPoint self-reference ────────────────────────────────────
            modelBuilder.Entity<LaunchPoint>()
                .HasOne(lp => lp.Parent)
                .WithMany(lp => lp.Children)
                .HasForeignKey(lp => lp.ParentID)
                .OnDelete(DeleteBehavior.Restrict);


            // MenuFeature
            modelBuilder.Entity<MenuFeature>()
                .HasIndex(e => new { e.MenuID, e.Code })
                .IsUnique();

            // UserPermission — unique (UserID, MenuID, FeatureCode) di DB pakai COALESCE
            modelBuilder.Entity<UserPermission>()
                .HasIndex(e => new { e.UserID, e.MenuID, e.FeatureCode });

            // TemplatePermission
            modelBuilder.Entity<TemplatePermission>()
                .HasIndex(e => new { e.TemplateName, e.MenuID, e.FeatureCode });

        }
    }
}
