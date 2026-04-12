

using Marilog.Shared.UI.Models;

namespace Marilog.Shared.UI.Layout
{
    public partial class MainLayout
    {
        private bool drawerOpen = true;

        private void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
        }

        private List<NavItemVm>? Items;

        protected override async Task OnInitializedAsync()
        {
            Items = new List<NavItemVm>(); 
            //Items = await NavService.GetAsync();
        }
    }
}
