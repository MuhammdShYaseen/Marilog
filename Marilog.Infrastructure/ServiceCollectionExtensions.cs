using Marilog.Application.EventHandlers;
using Marilog.Application.Interfaces.DataManagment;
using Marilog.Application.Interfaces.Encryption;
using Marilog.Application.Interfaces.Events;
using Marilog.Application.Interfaces.LogService;
using Marilog.Application.Interfaces.MapTiles;
using Marilog.Application.Interfaces.UserIdentity;
using Marilog.Application.Interfaces.VesselPosition;
using Marilog.Application.Interfaces.Weather;
using Marilog.Application.Options;
using Marilog.Application.Services.ApplicationServices.Encryption;
using Marilog.Application.Services.ApplicationServices.VesselPositions;
using Marilog.Contracts.Interfaces.DataManagment;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Contracts.Interfaces.Services.Infrastructure;
using Marilog.Contracts.Interfaces.Services.VesselPositions;
using Marilog.Contracts.Interfaces.UserIdenty;
using Marilog.Contracts.Options;
using Marilog.Domain.Events;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.BackgroundServices;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Dispatchers;
using Marilog.Infrastructure.Interfaces.EmailNotification;
using Marilog.Infrastructure.Models.Weather;
using Marilog.Infrastructure.Persistence;
using Marilog.Infrastructure.Repositories;
using Marilog.Infrastructure.Services;
using Marilog.Infrastructure.Services.DataBackup;
using Marilog.Infrastructure.Services.EmailNotification;
using Marilog.Infrastructure.Services.MapTiles;
using Marilog.Infrastructure.Services.UserIdenty;
using Marilog.Infrastructure.Services.VesselPositions;
using Marilog.Infrastructure.Services.Weather;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Marilog.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ── DbContext ─────────────────────────────────────────────────────────
            services.AddDbContext<MarilogContext>(options =>       // ← اسم الـ Context الفعلي
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                         sql => sql
                    .MigrationsAssembly("Marilog.Infrastructure")  // ← اسم المشروع الفعلي
                    .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

            // ── Generic Repository — covers all Aggregate Roots ───────────────────
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ILogReaderService, LogReaderService>();


            //-------encrption------------------
            services.AddScoped<ISecretEncryptionService>(_ =>
            {
                var key = configuration["Encryption:Key"]
                    ?? throw new InvalidOperationException(
                        "Encryption:Key is missing.");

                return new SecretEncryptionService(key);
            });
            //Event Dispatcher
            services.AddSingleton<InMemoryEventDispatcher>();

            services.AddSingleton<IEventDispatcher>(sp => sp.GetRequiredService<InMemoryEventDispatcher>());

            //Background
            services.AddHostedService<DomainEventProcessor>();

            //Event Handlers
            services.AddScoped<IEventHandler<StoredFileOcrRequestedEvent>, StoredFileOcrRequestedEventHandler>();

            //Local Storg
            services.AddSingleton<IFileStorageProvider, LocalFileStorageProvider>();

            //options
            services.Configure<UrlsOptions>(configuration.GetSection("Urls"));
            services.Configure<InternalApiKeysOptions>(configuration.GetSection("InternalApiKeys"));

            //backup
            services.AddScoped<IDatabaseBackupService>(sp =>
            {
                var provider = configuration["Database:Provider"];
                return provider switch
                {
                    "SqlServer" => ActivatorUtilities.CreateInstance<SqlServerBackupService>(sp),
                    //"PostgreSql" => ActivatorUtilities.CreateInstance<PostgreSqlBackupService>(sp),
                    _ => throw new NotSupportedException($"Provider not supported: {provider}")
                };
            });

            services.AddSingleton<IFileStorageBackupService, FileStorageBackupService>();
            services.AddScoped<ISchemaVersionProvider, EfCoreSchemaVersionProvider>();
            services.AddScoped<IFullBackupService, FullBackupService>();

            //=====Email=====================================================================
            


            //=====OCR========================================================================
            services.AddOcr(configuration);

            //=====EmailNotificationConfig====================================================
            services.AddSingleton<INotificationRecipientStore, JsonNotificationRecipientStore>();
            services.AddSingleton<INotificationSettingsStore, JsonNotificationSettingsStore>();
            services.AddScoped<INotificationSenderEmailSettingsStore, JsonNotificationSenderEmailSettingsStore>();
            services.AddScoped<INotificationEmailSender, NotificationEmailSender>();
            services.AddScoped<INotificationSchedule, NotificationSchedule>();
            services.AddSingleton<INotificationScheduleChangeNotifier, NotificationScheduleChangeNotifier>();
            services.AddHostedService<DailyNotificationBackgroundService>();


            //===VesselFinder===================================================================
            services.AddMemoryCache();

            services.AddHttpClient<IVesselPositionProvider, VesselFinderPositionProvider>(c =>
            {
                c.BaseAddress = new Uri("https://www.vesselfinder.com/");
                c.Timeout = TimeSpan.FromSeconds(15);
                c.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0 Safari/537.36");
            });

            services.AddScoped<IVesselPositionService, VesselPositionService>();



            //====Weather=========================================================================
            services.Configure<OpenMeteoOptions>(configuration.GetSection(OpenMeteoOptions.SectionName));
            services.AddHttpClient<IWeatherProvider, OpenMeteoWeatherProvider>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Marilog/1.0");
            });

            

            //=======Map=============================================================================
            services.Configure<MapTileOptions>(configuration.GetSection(MapTileOptions.SectionName));
            services.AddSingleton<IMapTileStore, FileSystemMapTileStore>();
            services.AddSingleton<IMapTileSource, HttpMapTileSource>();
            services.AddHttpClient(HttpMapTileSource.HttpClientName, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Marilog/1.0");
            });
            services.AddHostedService<MapTileSeeder>();


            //======UserIdenty=======================================================================
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;


        }
    }
}
