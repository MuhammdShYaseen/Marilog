using System.Windows;
using Marilog.Shared.UI.Extensions;
using Marilog.Ship.Data;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace Marilog.Ship
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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