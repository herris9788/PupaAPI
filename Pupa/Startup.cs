using Pupa.Configs;
using Pupa.BusinessObjects;
using Pupa.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Pupa
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
            services.AddLocalization();

            services.AddHttpContextAccessor();
            services.AddScoped<BeesuiteConnectionResolver>();
            services.AddDbContext<BeesuiteDbContext>((sp, options) =>
            {
                var conn = sp.GetRequiredService<BeesuiteConnectionResolver>().ConnectionString;
                options.UseNpgsql(conn);
                options.UseLazyLoadingProxies();
            });

            // Map each generic CustomDataController<TEntity> to its entity-set route
            // (must be an application-model provider so it runs before OData routing).
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<Microsoft.AspNetCore.Mvc.ApplicationModels.IApplicationModelProvider, GenericControllerNameProvider>());

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                })
                .ConfigureApplicationPartManager(partManager =>
                {
                    partManager.FeatureProviders.Add(new CustomGenericControllerFeatureProvider(services));
                })
                .AddOData((options, serviceProvider) =>
                {
                    TimeZoneInfo.ClearCachedData();
                    options.TimeZone = TimeZoneInfo.Local;
                    options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(1000);

                    options
                        .AddRouteComponents("beesuite/odata", BeesuiteEdmModel.GetEdmModel(), odataServices =>
                        {
                            odataServices.AddSingleton<ODataQueryValidator, OdataValidator>();
                        })
                        .EnableQueryFeatures(100);
                }).AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.MaxDepth = 100;
                });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Pupa.Configuration.JWTIssuer,
                    ValidAudience = Pupa.Configuration.JWTIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Pupa.Configuration.JWTSecret)),
                    ClockSkew = TimeSpan.Zero
                };
            });
            services.AddSwaggerGen(c =>
            {
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.OperationFilter<SwaggerDefaultValues>();
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, new string[] { } } });
                c.EnableAnnotations();
                c.OperationFilter<AddRequiredHeaderParameter>();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Pupa API",
                    Version = "v1",
                    Description = "Pupa API."
                });
            });
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.IncludeFields = true;
                o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });
            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.IncludeFields = true;
            });
            services.AddCors(x => x.AddPolicy("AllowSpecificOrigin", policyBuilder =>
            {
                policyBuilder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed((host) => true)
            .AllowCredentials();
            }));
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddScoped<IFtpService, FtpService>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "172.16.9.29:6379,password=Hastalavista,defaultDatabase=0";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            var supportedCultures = new[]{
                new CultureInfo("id-ID")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("id-ID"),
                SupportedCultures = supportedCultures,
                FallBackToParentCultures = false
            });
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("id-ID");

            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DisplayOperationId();
                c.DefaultModelsExpandDepth(-1);
                c.DocExpansion(DocExpansion.None);
                c.EnableFilter();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pupa API");
                c.EnablePersistAuthorization();
            });

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            app.UseRequestLocalization();

            if (!env.IsDevelopment())
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                    Path.Combine(Pupa.Configuration.OverseaAttachmentPath)),
                    RequestPath = "/Oversea/Files"
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                    Path.Combine(Pupa.Configuration.OverseaAttachmentPath)),
                    RequestPath = "/Public/Files"
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine("/home/ict/uploads")
                    ),
                    RequestPath = "/Public/HazardObservation"
                });
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
