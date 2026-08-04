

namespace Marilog.Infrastructure.Interfaces.OCR
{
    public interface IPdfConversionService
    {
        Task<bool> EnsurePdfAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
