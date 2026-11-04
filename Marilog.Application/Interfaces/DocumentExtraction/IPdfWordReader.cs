using Marilog.Application.Models.DocumentExtraction;

namespace Marilog.Application.Interfaces.DocumentExtraction
{
    public interface IPdfWordReader
    {
        IReadOnlyList<PdfPageWords> Read(byte[] pdf);
    }
}
