namespace Marilog.Web.Client.Layout
{
    public partial class MainLayout
    {
        private bool drawerOpen = true;

        private void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
        }
    }
}
