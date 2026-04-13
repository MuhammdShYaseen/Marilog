

using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Shared.UI.Layout
{
    public partial class MainLayout
    {
        private bool drawerOpen = true;

        private void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
        }

        private List<NavItemResponse>? Items;

        protected override async Task OnInitializedAsync()
        {
            Items = new List<NavItemResponse>(); 
            //Items = await NavService.GetAsync();
        }
    }
}
