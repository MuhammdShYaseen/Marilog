namespace Marilog.Shared.UI.Pages.Voyages
{
    public enum VoyageSection { Overview, Stops, BillsOfLading, Documents }

    public static class VoyageSections
    {
        public static string Slug(VoyageSection section) => section switch
        {
            VoyageSection.Stops => "stops",
            VoyageSection.BillsOfLading => "bills",
            VoyageSection.Documents => "documents",
            _ => "overview"
        };

        public static VoyageSection FromSlug(string? slug) => slug?.ToLowerInvariant() switch
        {
            "stops" => VoyageSection.Stops,
            "bills" => VoyageSection.BillsOfLading,
            "documents" => VoyageSection.Documents,
            _ => VoyageSection.Overview
        };

        public static string Url(int voyageId, VoyageSection section)
            => section == VoyageSection.Overview
                ? $"/voyages/{voyageId}"
                : $"/voyages/{voyageId}/{Slug(section)}";
    }
}