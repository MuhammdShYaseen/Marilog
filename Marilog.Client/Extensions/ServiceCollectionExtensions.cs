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
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICrewContractService, CrewContractService>();
            services.AddScoped<ICrewPayrollService, CrewPayrollService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            //>>>Alot of service should be here
            return services;
        }
    }
}
