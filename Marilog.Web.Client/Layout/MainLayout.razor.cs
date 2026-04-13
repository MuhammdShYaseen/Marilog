using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.FrontendServices;

namespace Marilog.Web.Client.Layout
{
    public partial class MainLayout
    {
        private bool drawerOpen = true;
        private readonly INavigationService _navService;
        public MainLayout(INavigationService navigationService)
        {
            _navService = navigationService;
        }

        private void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
        }

        private List<NavItemResponse>? Items;

        protected override async Task OnInitializedAsync()
        {
            Items = await _navService.GetAsync();
        }
    }


}
