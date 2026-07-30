
namespace Marilog.Infrastructure.OCR.Interfaces
{
    public interface IThumbnailGenerator
    {
        bool CanGenerate(string contentType);
        Task<string?> GenerateAsync(string sourceFullPath, CancellationToken ct = default);
    }
}
