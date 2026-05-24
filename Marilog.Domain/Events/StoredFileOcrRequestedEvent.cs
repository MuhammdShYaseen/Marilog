// Marilog.Domain/Events/DocumentOcrRequestedEvent.cs

using Marilog.Domain.Common;

namespace Marilog.Domain.Events;

public sealed class StoredFileOcrRequestedEvent : IDomainEvent
{
    public int StoredFileId { get; }
    public string FilePath { get; }

    public StoredFileOcrRequestedEvent(int storedFileId, string filePath)
    {
        StoredFileId = storedFileId;
        FilePath = filePath;
    }
}