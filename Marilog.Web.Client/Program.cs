using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Marilog.Client.Extensions;
using Marilog.Shared.UI.Extensions;
namespace Marilog.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001/")
            });
            builder.Services.AddMudServices();
            builder.Services.AddMarilogClientService();
            //builder.Services.AddMarilogUI();
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            await builder.Build().RunAsync();
        }
    }
}
