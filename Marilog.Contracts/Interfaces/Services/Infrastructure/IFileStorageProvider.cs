

namespace Marilog.Contracts.Interfaces.Services.Infrastructure
{
    public interface IFileStorageProvider
    {
        Task<string> SaveAsync(Stream stream, string fileName, CancellationToken ct = default);
        Task<Stream> ReadAsync(string relativePath, CancellationToken ct = default);
        Task DeleteAsync(string relativePath, CancellationToken ct = default);
        string? GetRelativePath(string? fullPath);
    }
}
