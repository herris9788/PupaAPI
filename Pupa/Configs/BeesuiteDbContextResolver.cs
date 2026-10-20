using Microsoft.EntityFrameworkCore;
using Models.CustomOData;
using Pupa.BusinessObjects;

namespace Pupa.Configs
{
    /// <summary>Pupa has a single EF Core context, so every entity resolves to it.</summary>
    public sealed class BeesuiteDbContextResolver : IEntityDbContextResolver
    {
        private readonly BeesuiteDbContext _db;
        public BeesuiteDbContextResolver(BeesuiteDbContext db) { _db = db; }
        public DbContext GetDbContext(Type entityType) => _db;
    }
}
