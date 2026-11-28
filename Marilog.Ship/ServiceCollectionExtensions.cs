using Marilog.Application.EventHandlers;
using Marilog.Application.Helpers;
using Marilog.Application.Interfaces.Events;
using Marilog.Application.Interfaces.OCR;
using Marilog.Application.Interfaces.OnlineCurrencyRate;
using Marilog.Application.Services.ApplicationServices.SystemServices;
using Marilog.Contracts.Interfaces.Services.Infrastructure;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Contracts.Options;
using Marilog.Domain.Events;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.Interfaces.OCR;
using Marilog.Infrastructure.Services.OCR;
using Marilog.Ship.Data;
using Marilog.Ship.Services;
using Marilog.Ship.Services.Files;
using Marilog.Ship.Services.Purchases;
using Marilog.Ship.Services.Setup;
using Marilog.Ship.Services.Unlock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marilog.Ship
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShip(this IServiceCollection services)
        {
            services.AddLogging();

            // ── DbContext (SQLite file on the ship PC) ───────────────────────────
            services.AddDbContext<ShipContext>(options =>
                options.UseSqlite(ShipDatabase.ConnectionString));

            services.AddScoped(typeof(IRepository<>), typeof(ShipRepository<>));

            // Domain events after a save (same idea as Marilog's InMemoryEventDispatcher).
            services.AddSingleton<IEventDispatcher, ShipEventDispatcher>();
            services.AddScoped<IEventHandler<StoredFileOcrRequestedEvent>, StoredFileOcrRequestedEventHandler>();

            // ── Marilog application services, used in-process ────────────────────
            services.AddScoped<IVesselService, VesselService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IVesselTypeService, VesselTypeService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<ICrewContractService, CrewContractService>();
            services.AddScoped<IRankService, RankService>();
            services.AddScoped<IPortService, PortService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();

            // CurrencyService: rates are entered by hand on board (no online refresh).
            services.AddOptions<ExchangeRatesOptions>();
            services.AddSingleton<ExchangeRateRefreshGate>();
            services.AddScoped<IExchangeRateProvider, OfflineExchangeRateProvider>();
            services.AddScoped<ICurrencyService, CurrencyService>();

            // ── Attached files ───────────────────────────────────────────────────
            // StoredFileService as in Marilog; files on the ship PC; text/previews without OCR.
            services.AddSingleton<IFileStorageProvider, ShipFileStorageProvider>();
            services.AddSingleton<IPdfConversionService, PdfConversionService>();
            services.AddSingleton<IPdfDirectTextExtractor, PdfDirectTextExtractor>();
#pragma warning disable CA1416 // Windows only
            services.AddSingleton<IThumbnailGenerator, PdfThumbnailGenerator>();
#pragma warning restore CA1416
            services.AddScoped<IOcrStarter, ShipFileProcessor>();
            services.AddScoped<IStoredFileService, StoredFileService>();

            // ── Ship services ────────────────────────────────────────────────────
            services.AddScoped<IShipSetupService, ShipSetupService>();
            services.AddScoped<IUnlockService, UnlockService>();
            services.AddScoped<IPurchaseService, PurchaseService>();

            return services;
        }
    }
}