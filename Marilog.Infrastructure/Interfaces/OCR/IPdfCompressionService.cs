namespace Marilog.Infrastructure.Interfaces.OCR
{
    public interface IPdfCompressionService
    {
        Task<bool> CompressAsync(string pdfPath, CancellationToken ct = default);
    }
}
