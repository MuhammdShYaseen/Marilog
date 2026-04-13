using Marilog.Shared.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace Marilog.Shared.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarilogUI(this IServiceCollection services)
        {
            services.AddMudServices();
            services.AddSingleton<NavigationStateService>();
            return services;
        }
    }
}
