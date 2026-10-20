

namespace Marilog.Application.Options
{
    public sealed class MapTileOptions
    {
        public const string SectionName = "MapTiles";

        public string StoragePath { get; set; } = "App_Data/map-tiles";
        public int MaxZoom { get; set; } = 13;
        public int SeedMaxZoom { get; set; } = 5;          // -1 disables seeding
        public int SeedDelayMs { get; set; } = 1500;
        public int UpstreamCooldownMinutes { get; set; } = 15;

        // key (lowercase) -> upstream URL template with {z}/{y}/{x}
        public Dictionary<string, string> Layers { get; set; } = new();
    }
}
