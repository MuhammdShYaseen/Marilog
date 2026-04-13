using Marilog.Client.Services;
using Marilog.Contracts.Interfaces.FrontendServices;
using Microsoft.Extensions.DependencyInjection;

namespace Marilog.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarilogClientService(this IServiceCollection services)
        {
            services.AddScoped<INavigationService, NavigationService>();
            //>>>Alot of service should be here
            return services;
        }
    }
}
