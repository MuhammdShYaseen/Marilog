// Marilog.Domain/Events/DocumentOcrRequestedEvent.cs

using Marilog.Domain.Common;

namespace Marilog.Domain.Events;

public sealed class StoredFileOcrRequestedEvent : IDomainEvent
{
    public Guid StoredFileId { get; }
    public string FilePath { get; }

    public StoredFileOcrRequestedEvent(Guid storedFileId, string filePath)
    {
        StoredFileId = storedFileId;
        FilePath = filePath;
    }
}