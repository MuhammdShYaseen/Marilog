namespace Marilog.Infrastructure.Interfaces.OCR
{
    public interface IThumbnailGenerator
    {
        bool CanGenerate(string contentType);
        Task<string?> GenerateAsync(string sourceFullPath, CancellationToken ct = default);
    }
}
