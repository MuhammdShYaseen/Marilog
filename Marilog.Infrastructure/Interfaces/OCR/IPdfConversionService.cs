

namespace Marilog.Infrastructure.Interfaces.OCR
{
    public interface IPdfConversionService
    {
        Task<string?> EnsurePdfAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
