namespace Pupa.Configs
{
    public class BeesuiteConnectionResolver
    {
        public string ConnectionString { get; }

        public BeesuiteConnectionResolver(IHttpContextAccessor accessor, IConfiguration config)
        {
            var dbKey = accessor.HttpContext?.Request.Headers["X-API-DB"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(dbKey))
                dbKey = "Beesuite";

            // Mode STAGING: web menambah "_TEST" di belakang X-API-DB (mis. AS_WNS_TEST) -- SQL Server memakai DB *_TEST itu
            // apa adanya, sedangkan Postgres di sini dialihkan ke connection "Beesuite2" (database beesuite_staging).
            if (dbKey.EndsWith("_TEST", StringComparison.OrdinalIgnoreCase))
                dbKey = "Beesuite2";

            ConnectionString = config.GetConnectionString(dbKey)
                            ?? config.GetConnectionString("Beesuite")!;
        }
    }
}
