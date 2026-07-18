using Marilog.Client.Extensions;
using Marilog.Contracts.Options;
using Marilog.Shared.UI.Extensions;
using Marilog.Web.Client.Services.Implementation;
using Marilog.Web.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
namespace Marilog.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var apiPort = builder.Configuration["ApiSettings:Port"] ?? "5001";
            var host = builder.HostEnvironment.BaseAddress;
            var uri = new Uri(host);

            string apiBase;

            if (uri.Host.Contains(".devtunnels.ms"))
            {
                // Dev Tunnel: استبدل الـ port في الـ subdomain
                // من: m7bcrg1w-5003.euw.devtunnels.ms
                // إلى: m7bcrg1w-5001.euw.devtunnels.ms
                var newHost = System.Text.RegularExpressions.Regex.Replace(
                    uri.Host,
                    @"-\d+\.",
                    $"-{apiPort}."
                );
                apiBase = $"{uri.Scheme}://{newHost}/";
            }
            else
            {
                // Local: استخدم نفس host مع port مختلف
                apiBase = $"{uri.Scheme}://{uri.Host}:{apiPort}/";
            }

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(apiBase)
            });
            //builder.Services.AddMudServices();
            builder.Services.AddMarilogClientService();
            builder.Services.AddMarilogUI();
            builder.Services.AddScoped<IManageStoredFiles, ManageStoredFiles>();
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.Configure<UrlsOptions>(builder.Configuration.GetSection("Urls"));
            await builder.Build().RunAsync();
        }
    }
}