using Marilog.Application.EventHandlers;
using Marilog.Application.Interfaces.Encryption;
using Marilog.Application.Interfaces.Events;
using Marilog.Application.Interfaces.LogService;
using Marilog.Application.Services.ApplicationServices.Encryption;
using Marilog.Contracts.Interfaces.Services.Infrastructure;
using Marilog.Contracts.Options;
using Marilog.Domain.Events;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Dispatchers;
using Marilog.Infrastructure.Repositories;
using Marilog.Infrastructure.Services;
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

            //Event Handlers
            services.AddScoped<IEventHandler<StoredFileOcrRequestedEvent>, StoredFileOcrRequestedEventHandler>();

            //Local Storg
            services.AddSingleton<IFileStorageProvider, LocalFileStorageProvider>();

            //options
            services.Configure<UrlsOptions>(configuration.GetSection("Urls"));

            return services;
        }
    }
}
