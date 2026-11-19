namespace Pupa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Limits.KeepAliveTimeout = TimeSpan.FromHours(1);
                        // Quick Order / Item Request send attachments as base64 JSON; the 30 MB default rejected a 23 MB PDF set.
                        options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
                    })
                    .UseUrls("http://0.0.0.0:3011");
                });
    }
}
