// Marilog.Domain/Events/DocumentOcrRequestedEvent.cs

using Marilog.Domain.Common;

namespace Marilog.Domain.Events;

public sealed class DocumentOcrRequestedEvent : IDomainEvent
{
    public int DocumentId { get; }
    public string FilePath { get; }

    public DocumentOcrRequestedEvent(int documentId, string filePath)
    {
        DocumentId = documentId;
        FilePath = filePath;
    }
}