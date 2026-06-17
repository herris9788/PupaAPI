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

            ConnectionString = config.GetConnectionString(dbKey)
                            ?? config.GetConnectionString("Beesuite")!;
        }
    }
}
