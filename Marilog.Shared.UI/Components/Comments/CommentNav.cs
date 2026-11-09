using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Components.Comments
{
    // Which entity types' comments are counted on each nav menu route.
    public static class CommentNav
    {
        private static readonly Dictionary<string, EntityType[]> ByRoute = new(StringComparer.OrdinalIgnoreCase)
        {
            ["voyages"] = [EntityType.Voyage, EntityType.VoyageStop],
            ["documents"] = [EntityType.Document],
            ["swift-transfers"] = [EntityType.SwiftTransfer],
            ["contracts"] = [EntityType.AContract],
            ["companies"] = [EntityType.Company],
            ["inbox-entry"] = [EntityType.InboxEntry],
            ["vessels"] = [EntityType.Vessel, EntityType.VesselCertificate],
            ["vessel-certificates"] = [EntityType.VesselCertificate],
            ["crew-contracts"] = [EntityType.CrewContract],
            ["persons"] = [EntityType.Person, EntityType.PersonCertificate],
            ["person-certificates"] = [EntityType.PersonCertificate],
            ["payroll"] = [EntityType.CrewPayroll],
            ["admin/banks"] = [EntityType.Bank],
        };

        // A group counts the union of its children's types, so nothing is counted twice.
        public static IReadOnlyCollection<EntityType> TypesFor(NavItemResponse item)
        {
            if (item.Children.Count > 0)
                return item.Children.SelectMany(TypesFor).Distinct().ToList();

            var key = item.Route?.Trim().Trim('/') ?? string.Empty;
            return ByRoute.TryGetValue(key, out var types) ? types : [];
        }
    }
}