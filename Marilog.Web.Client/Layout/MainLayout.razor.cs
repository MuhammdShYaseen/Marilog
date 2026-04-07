using Marilog.Web.Client.Models;

namespace Marilog.Web.Client.Layout
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
            Items = await NavService.GetAsync();
        }
    }


}
