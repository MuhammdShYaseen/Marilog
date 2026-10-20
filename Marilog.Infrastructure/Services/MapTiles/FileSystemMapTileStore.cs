using Marilog.Application.Interfaces.MapTiles;
using Marilog.Application.Options;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Marilog.Infrastructure.Services.MapTiles
{
    internal sealed class FileSystemMapTileStore : IMapTileStore
    {
        private readonly string _root;

        public FileSystemMapTileStore(IOptions<MapTileOptions> options)
        {
            var path = options.Value.StoragePath;
            _root = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        }

        public async Task<byte[]?> ReadAsync(string layer, int z, int x, int y, CancellationToken cancellationToken)
        {
            var path = TilePath(layer, z, x, y);
            if (!File.Exists(path))
                return null;

            try
            {
                return await File.ReadAllBytesAsync(path, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public bool IsMarkedMissing(string layer, int z, int x, int y) =>
            File.Exists(MissingPath(layer, z, x, y));

        public bool Exists(string layer, int z, int x, int y) =>
            File.Exists(TilePath(layer, z, x, y)) || File.Exists(MissingPath(layer, z, x, y));

        public Task WriteAsync(string layer, int z, int x, int y, byte[] content, CancellationToken cancellationToken) =>
            WriteAtomicAsync(TilePath(layer, z, x, y), content, cancellationToken);

        public Task MarkMissingAsync(string layer, int z, int x, int y, CancellationToken cancellationToken) =>
            WriteAtomicAsync(MissingPath(layer, z, x, y), [], cancellationToken);

        private static async Task WriteAtomicAsync(string path, byte[] content, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
            await File.WriteAllBytesAsync(tempPath, content, cancellationToken);
            File.Move(tempPath, path, overwrite: true);
        }

        private string TilePath(string layer, int z, int x, int y) => BasePath(layer, z, x, y) + ".tile";

        private string MissingPath(string layer, int z, int x, int y) => BasePath(layer, z, x, y) + ".missing";

        private string BasePath(string layer, int z, int x, int y) =>
            Path.Combine(
                _root,
                layer,
                z.ToString(CultureInfo.InvariantCulture),
                x.ToString(CultureInfo.InvariantCulture),
                y.ToString(CultureInfo.InvariantCulture));
    }
}
