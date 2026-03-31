

namespace Marilog.Application.DTOs.Commands.Document
{
    public record CreateDocumentTypeCommand(
    string Code,
    string Name,
    int SortOrder = 0
);
}
