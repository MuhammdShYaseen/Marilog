
namespace Marilog.Infrastructure.OCR.Interfaces
{
    public interface IPdfDirectTextExtractor
    {
        string ExtractText(string filePath, CancellationToken cancellationToken = default);
    }
}
