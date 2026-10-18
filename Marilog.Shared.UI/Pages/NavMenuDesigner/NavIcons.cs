using MudBlazor;
namespace Marilog.Shared.UI.Pages.NavMenuDesigner
{
    public static class NavIcons
    {
        private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
        {
            // ── Existing keys (already stored in the DB) ─────────────────────────
            ["Dashboard"] = Icons.Material.Filled.Dashboard,
            ["DirectionsBoat"] = Icons.Material.Filled.DirectionsBoat,
            ["Route"] = Icons.Material.Filled.Route,
            ["People"] = Icons.Material.Filled.People,
            ["AccountBalance"] = Icons.Material.Filled.AccountBalance,
            ["Assessment"] = Icons.Material.Filled.Assessment,
            ["Storage"] = Icons.Material.Filled.Storage,
            ["Description"] = Icons.Material.Filled.Description,
            ["Payments"] = Icons.Material.Filled.Payments,
            ["Email"] = Icons.Material.Filled.Email,
            ["Settings"] = Icons.Material.Filled.Settings,
            ["Apartment"] = Icons.Material.Filled.Apartment,
            ["LocationOn"] = Icons.Material.Filled.LocationOn,
            ["Business"] = Icons.Material.Filled.Business,
            ["Person"] = Icons.Material.Filled.Person,
            ["Map"] = Icons.Material.Filled.Map,
            ["Badge"] = Icons.Material.Filled.Badge,
            ["Folder"] = Icons.Material.Filled.Folder,
            ["SwapHoriz"] = Icons.Material.Filled.SwapHoriz,
            ["Security"] = Icons.Material.Filled.Security,
            ["Public"] = Icons.Material.Filled.Public,
            ["AttachMoney"] = Icons.Material.Filled.AttachMoney,
            ["log"] = Icons.Material.Filled.ReceiptLong,
            ["calculator"] = Icons.Material.Filled.Calculate,
            ["bank"] = Icons.Material.Filled.Business,
            ["pdf"] = Icons.Material.Filled.PictureAsPdf,
            ["backup"] = Icons.Material.Filled.Backup,
            ["inbox"] = Icons.Material.Filled.Inbox,
            ["outbox"] = Icons.Material.Filled.Outbox,
            ["draft"] = Icons.Material.Filled.Drafts,
            ["account"] = Icons.Material.Filled.ManageAccounts,
            ["notification"] = Icons.Material.Filled.Notifications,
            ["Credit"] = Icons.Material.Filled.CreditCard,
            ["theme"] = Icons.Material.Filled.Palette,
            ["rank"] = Icons.Material.Filled.LocalPolice,

            // ── New choices for the designer ─────────────────────────────────────
            ["Home"] = Icons.Material.Filled.Home,
            ["LocalShipping"] = Icons.Material.Filled.LocalShipping,
            ["Receipt"] = Icons.Material.Filled.Receipt,
            ["Article"] = Icons.Material.Filled.Article,
            ["AccountBalanceWallet"] = Icons.Material.Filled.AccountBalanceWallet,
            ["Groups"] = Icons.Material.Filled.Groups,
            ["AttachFile"] = Icons.Material.Filled.AttachFile,
            ["Inventory"] = Icons.Material.Filled.Inventory,
            ["Event"] = Icons.Material.Filled.Event,
            ["Schedule"] = Icons.Material.Filled.Schedule,
            ["Timeline"] = Icons.Material.Filled.Timeline,
            ["Tune"] = Icons.Material.Filled.Tune,
            ["AdminPanelSettings"] = Icons.Material.Filled.AdminPanelSettings,
            ["Category"] = Icons.Material.Filled.Category
        };

        public static IReadOnlyList<string> Names { get; } =
            Map.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToArray();

        public static string? Resolve(string? name)
            => name is not null && Map.TryGetValue(name, out var svg) ? svg : null;
    }
}
