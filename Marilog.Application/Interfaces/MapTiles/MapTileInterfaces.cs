using Marilog.Kernel.Enums;

namespace Marilog.Application.Interfaces.MapTiles
{
    public sealed record UpstreamTileResult(UpstreamTileStatus Status, byte[]? Content)
    {
        public static UpstreamTileResult Ok(byte[] content) => new(UpstreamTileStatus.Ok, content);
        public static readonly UpstreamTileResult NotFound = new(UpstreamTileStatus.NotFound, null);
        public static readonly UpstreamTileResult Failed = new(UpstreamTileStatus.Failed, null);
    }

    public interface IMapTileSource
    {
        Task<UpstreamTileResult> FetchAsync(string urlTemplate, int z, int x, int y, CancellationToken cancellationToken);
    }

    public interface IMapTileStore
    {
        Task<byte[]?> ReadAsync(string layer, int z, int x, int y, CancellationToken cancellationToken);
        bool IsMarkedMissing(string layer, int z, int x, int y);
        bool Exists(string layer, int z, int x, int y);
        Task WriteAsync(string layer, int z, int x, int y, byte[] content, CancellationToken cancellationToken);
        Task MarkMissingAsync(string layer, int z, int x, int y, CancellationToken cancellationToken);
    }

    public static class TileImage
    {
        public static string? DetectContentType(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";

            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "image/jpeg";

            return null;
        }
    }
}
