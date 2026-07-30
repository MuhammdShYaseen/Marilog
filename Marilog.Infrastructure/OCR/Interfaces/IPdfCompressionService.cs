
namespace Marilog.Infrastructure.OCR.Interfaces
{
    public interface IPdfCompressionService
    {
        Task<bool> CompressAsync(string pdfPath, CancellationToken ct = default);
    }
}
