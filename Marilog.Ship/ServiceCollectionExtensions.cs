using Marilog.Application.Helpers;
using Marilog.Application.Interfaces.OnlineCurrencyRate;
using Marilog.Application.Services.ApplicationServices.SystemServices;
using Marilog.Contracts.Options;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Ship.Data;
using Marilog.Ship.Services;
using Marilog.Ship.Services.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marilog.Ship
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShip(this IServiceCollection services)
        {
            // ── DbContext (SQLite file on the ship PC) ───────────────────────────
            services.AddDbContext<ShipContext>(options =>
                options.UseSqlite(ShipDatabase.ConnectionString));

            services.AddScoped(typeof(IRepository<>), typeof(ShipRepository<>));

            // ── Marilog application services, used in-process ────────────────────
            services.AddScoped<IVesselService, VesselService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IVesselTypeService, VesselTypeService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<ICrewContractService, CrewContractService>();
            services.AddScoped<IRankService, RankService>();
            services.AddScoped<IPortService, PortService>();

            // CurrencyService: rates are entered by hand on board (no online refresh).
            services.AddOptions<ExchangeRatesOptions>();
            services.AddSingleton<ExchangeRateRefreshGate>();
            services.AddScoped<IExchangeRateProvider, OfflineExchangeRateProvider>();
            services.AddScoped<ICurrencyService, CurrencyService>();

            // ── Ship services ────────────────────────────────────────────────────
            services.AddScoped<IShipSetupService, ShipSetupService>();

            return services;
        }
    }
}