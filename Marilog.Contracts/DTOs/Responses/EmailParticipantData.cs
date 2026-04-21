using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Responses
{
    public sealed record EmailParticipantData(
        ParticipantRole Role,
        ParticipantType ParticipantType,
        int ParticipantId,
        string? DisplayName = null,
        string? EmailAddress = null);
}
