using Marilog.Web.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Marilog.Web.Client.Shared
{
    public partial class NavMenu
    {
        [Parameter]
        public List<NavItemVm> Items { get; set; } = new();

        RenderFragment RenderItem(NavItemVm item) => builder =>
        {
            if (item.Children.Any())
            {
                builder.OpenComponent<MudNavGroup>(0);

                builder.AddAttribute(1, "Title", item.Title);

                builder.AddAttribute(2, "Icon",
                    GetIcon(item.Icon));

                builder.AddAttribute(3, "ChildContent",
                    (RenderFragment)(childBuilder =>
                    {
                        int seq = 0;

                        foreach (var child in item.Children)
                        {
                            childBuilder.AddContent(
                                seq++,
                                RenderItem(child));
                        }
                    }));

                builder.CloseComponent();
            }
            else
            {
                builder.OpenComponent<MudNavLink>(0);

                builder.AddAttribute(1, "Href",
                    item.Route);

                builder.AddAttribute(2, "Match",
        NavLinkMatch.All);

                builder.AddAttribute(3, "Icon",
                    GetIcon(item.Icon));

                builder.AddAttribute(4, "Class", "mud-inherit-text");

                builder.AddAttribute(5, "ChildContent",
                    (RenderFragment)(b =>
                    {
                        b.AddContent(6,
                            item.Title);
                    }));

                builder.CloseComponent();
            }
        };

        string GetIcon(string? icon) => icon switch
        {
            "Dashboard" => Icons.Material.Filled.Dashboard,
            "DirectionsBoat" => Icons.Material.Filled.DirectionsBoat,
            "Route" => Icons.Material.Filled.Route,
            "People" => Icons.Material.Filled.People,
            "AccountBalance" => Icons.Material.Filled.AccountBalance,
            "Assessment" => Icons.Material.Filled.Assessment,
            "Storage" => Icons.Material.Filled.Storage,
            "Description" => Icons.Material.Filled.Description,
            "Payments" => Icons.Material.Filled.Payments,
            "Email" => Icons.Material.Filled.Email,
            "Settings" => Icons.Material.Filled.Settings,



            _ => Icons.Material.Filled.Menu
        };
    }
}
