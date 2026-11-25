using Marilog.Application.Services.ApplicationServices.SystemServices;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Ship.Data;
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

            return services;
        }
    }
}