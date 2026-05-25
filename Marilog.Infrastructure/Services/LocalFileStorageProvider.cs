using Marilog.Contracts.Interfaces.Services.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Marilog.Infrastructure.Services
{
    public class LocalFileStorageProvider : IFileStorageProvider
    {
        private readonly string _basePath;

        public LocalFileStorageProvider(IConfiguration config)
        {
            _basePath = config["FileStorage:BasePath"]
                ?? throw new InvalidOperationException("FileStorage:BasePath not configured.");
        }

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
    }
}
