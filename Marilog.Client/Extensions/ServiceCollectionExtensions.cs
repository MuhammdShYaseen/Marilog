using Marilog.Client.Services;
using Marilog.Client.Services.SystemServices;
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
            //>>>Alot of service should be here
            return services;
        }
    }
}
