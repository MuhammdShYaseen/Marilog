namespace Marilog.Infrastructure.Interfaces.OCR
{
    public interface IPdfDirectTextExtractor
    {
        string ExtractText(string filePath, CancellationToken cancellationToken = default);
    }
}
