using Marilog.Shared.UI.Extensions;
using Marilog.Ship.Data;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;

namespace Marilog.Ship
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Same reason as InvariantGlobalization in Marilog.Web.Client: MudNumericField and the
            // date pickers must not follow the Windows regional settings of the ship PC.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif
            services.AddMarilogUI();   // MudBlazor + AppStateService (Shared.UI)
            services.AddShip();

            var provider = services.BuildServiceProvider();
            ShipDatabase.Initialize(provider);

            blazorWebView.Services = provider;

            // Added in code, not XAML: the XAML compiler can't always see Razor-generated components.
            blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(Routes)
            });
        }
    }
}