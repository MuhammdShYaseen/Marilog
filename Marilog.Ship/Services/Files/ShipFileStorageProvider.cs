using Marilog.Contracts.Interfaces.Services.Infrastructure;
using Marilog.Ship.Data;
using System.IO;

namespace Marilog.Ship.Services.Files
{
    /// <summary>
    /// Same as Marilog's LocalFileStorageProvider, with the base folder fixed to the ship's Files folder
    /// (no appsettings on board). Files go to Files\uploads\{guid}.{ext}.
    /// </summary>
    public class ShipFileStorageProvider : IFileStorageProvider
    {
        private readonly string _basePath = ShipDatabase.FilesFolder;

        public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken ct = default)
        {
            var relativePath = Path.Combine("uploads", fileName);
            var fullPath = Path.Combine(_basePath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            await using var fs = File.Create(fullPath);
            await stream.CopyToAsync(fs, ct);

            return relativePath;
        }

        public Task<Stream> ReadAsync(string relativePath, CancellationToken ct = default)
        {
            var fullPath = Path.Combine(_basePath, relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found on disk.", fullPath);

            Stream stream = File.OpenRead(fullPath);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string relativePath, CancellationToken ct = default)
        {
            var fullPath = Path.Combine(_basePath, relativePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }

        public string? GetRelativePath(string? fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return fullPath;

            return Path.GetRelativePath(_basePath, fullPath);
        }
    }
}