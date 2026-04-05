using Marilog.Application.EventHandlers;
using Marilog.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Marilog.Application.Services.ApplicationServices;
using Marilog.Application.Interfaces.FrontendServices;
using Marilog.Application.Services.FrontendServices;

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

            //---Contracts-----------------------
            services.AddScoped<IContractService, ContractService>();

            //----FrontendServices-------------------
            services.AddScoped<INavigationService, NavigationService>();

            string autoMapperKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODA1NDE0NDAwIiwiaWF0IjoiMTc3MzkxNDI2NyIsImFjY291bnRfaWQiOiIwMTlkMDU4NmVlZmM3OWRkYjg0MzQ5NjI0MTA1NDU5NSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa20ycmY3YmN2bjNoc3g4a3ZoY3EweW03Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.Pii8V4OkoaGgzQz0pPLlzMCyDKbZSvQAGJxssaFEK7OO-3HhYd1PD7iIcCZjNj0kQQEE_WLMXyacwpcW41sS4OkrVPbD2UCWmMz9cdwnE5aMNjckI4-EiRlo1i5iOcIcbJ3iO-KFwgnfQtyo0-qOBdNV7KTz3wOe_Nrak2_1UlZ0oQoJnKO4Bmp_Spoh9idNvSH1SjQLIb_HPiNnUK4PojiDk8QCw-wNecsKaxflls2VWa2qZGlcKi8nW6L6wDXX0YTcQM8WZWCGScxTedU0xT-ke-ag4AeuDWASvHKNyNvdJ_tXIG6QSna-LttHdeWiFA1LZ4oBeSmDWy6yejaTmQ";
            services.AddAutoMapper(cfg => cfg.LicenseKey = autoMapperKey, AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }
    }
}
