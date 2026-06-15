using Marilog.Client.Extensions;
using Marilog.Contracts.Options;
using Marilog.Shared.UI.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
namespace Marilog.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped(sp =>
            {
                var urls = sp.GetRequiredService<IOptions<UrlsOptions>>().Value;
                return new HttpClient
                {
                    BaseAddress = new Uri(urls.Presentation)
                };
            });
            //builder.Services.AddMudServices();
            builder.Services.AddMarilogClientService();
            builder.Services.AddMarilogUI();
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.Configure<UrlsOptions>(builder.Configuration.GetSection("Urls"));
            await builder.Build().RunAsync();
        }
    }
}