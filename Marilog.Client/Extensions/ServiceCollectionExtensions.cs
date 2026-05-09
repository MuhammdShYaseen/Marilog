using Marilog.Client.Interfaces;
using Marilog.Client.Services.SystemServices;
using Marilog.Client.Services.UiServices;
using Marilog.Contracts.Interfaces.FrontendServices;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.Extensions.DependencyInjection;

namespace Marilog.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarilogClientService(this IServiceCollection services)
        {
           
            //===system============================================================
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICrewContractService, CrewContractService>();
            services.AddScoped<ICrewPayrollService, CrewPayrollService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOfficeService, OfficeService>();
            services.AddScoped<IOperationsDashboardService, OperationsDashboardService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IPortService, PortService>();
            services.AddScoped<IRankService, RankService>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<ISwiftTransferService, SwiftTransferService>();
            services.AddScoped<IVesselService, VesselService>();
            services.AddScoped<IVoyageService, VoyageService>();

            //=====frontend================================================================
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IAppThemeService, AppThemeService>();

            return services;
        }
    }
}
