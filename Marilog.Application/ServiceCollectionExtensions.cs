using Marilog.Application.EventHandlers;
using Marilog.Application.Services;
using Marilog.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Marilog.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // ── Lookups ───────────────────────────────────────────────────────────
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IPortService, PortService>();
            services.AddScoped<IRankService, RankService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IOfficeService, OfficeService>();

            // ── Core ──────────────────────────────────────────────────────────────
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IVesselService, VesselService>();

            // ── Operations ────────────────────────────────────────────────────────
            services.AddScoped<ICrewContractService, CrewContractService>();
            services.AddScoped<IVoyageService, VoyageService>();


            // ── Financial ─────────────────────────────────────────────────────────
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ISwiftTransferService, SwiftTransferService>();

            // ── Payroll ───────────────────────────────────────────────────────────
            services.AddScoped<ICrewPayrollService, CrewPayrollService>();

            // ── Communication ─────────────────────────────────────────────────────
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<DocumentEmailRequestedEventHandler>();


            // ── Domain Services ───────────────────────────────────────────────────
            services.AddScoped<IPayrollCalculatorService, PayrollCalculatorService>();
            services.AddScoped<IDocumentNumberService, DocumentNumberService>();
            return services;
        }
    }
}
