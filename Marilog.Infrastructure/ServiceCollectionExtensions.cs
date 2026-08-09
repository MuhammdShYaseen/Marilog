using Marilog.Application.EventHandlers;
using Marilog.Application.Interfaces.DataManagment;
using Marilog.Application.Interfaces.Email;
using Marilog.Application.Interfaces.Encryption;
using Marilog.Application.Interfaces.Events;
using Marilog.Application.Interfaces.LogService;
using Marilog.Application.Services.ApplicationServices.Encryption;
using Marilog.Contracts.Interfaces.DataManagment;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Contracts.Interfaces.Services.Infrastructure;
using Marilog.Contracts.Options;
using Marilog.Domain.Events;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.BackgroundServices;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Dispatchers;
using Marilog.Infrastructure.Persistence;
using Marilog.Infrastructure.Repositories;
using Marilog.Infrastructure.Services;
using Marilog.Infrastructure.Services.DataBackup;
using Marilog.Infrastructure.Services.Email.Factory;
using Marilog.Infrastructure.Services.Email.Google;
using Marilog.Infrastructure.Services.Email.Smtp;
using Marilog.Infrastructure.Services.EmailNotification;
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
            services.Configure<GoogleOAuthOptions>(configuration.GetSection("GoogleOAuth"));

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
            services.AddSingleton<ImapSmtpEmailProviderClient>();
            services.AddScoped<GoogleApiEmailProviderClient>();
            services.AddScoped<IEmailProviderClientFactory, EmailProviderClientFactory>();
            services.AddScoped<IGoogleOAuthTokenService, GoogleOAuthTokenService>();
            services.AddHostedService<MailSyncBackgroundService>();


            //=====OCR========================================================================
            services.AddOcr(configuration);

            //=====EmailNotificationConfig====================================================
            services.AddSingleton<INotificationRecipientStore, JsonNotificationRecipientStore>();
            return services;





        }
    }
}
