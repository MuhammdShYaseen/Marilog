using Marilog.Domain.Common;
using Marilog.Domain.Entities;

namespace Marilog.Domain.Events
{
    public sealed record EmailParticipantData(
        ParticipantRole Role,
        ParticipantType ParticipantType,
        int ParticipantId,
        string? DisplayName = null,
        string? EmailAddress = null);

    /// <summary>
    /// Raised when Document.LogEmail() is called.
    /// Carries all participant data needed to create the Email aggregate.
    /// </summary>
    public sealed record DocumentEmailRequestedEvent(
        int DocumentId,
        string DocNumber,
        string EntityType,
        string Subject,
        string Body,
        EmailDirection Direction,
        IReadOnlyList<EmailParticipantData> Participants) : IDomainEvent;
}
