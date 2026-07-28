using Marilog.Domain.Events;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;

namespace Marilog.Application.EventHandlers
{
    /// <summary>
    /// Handles DocumentEmailRequestedEvent raised by Document.LogEmail().
    /// Creates and persists the Email aggregate via IEmailService.
    /// Wire via MediatR INotificationHandler or your preferred dispatcher.
    /// </summary>
    public class DocumentEmailRequestedEventHandler
    {
        private readonly IEmailService _emailService;

        public DocumentEmailRequestedEventHandler(IEmailService emailService)
            => _emailService = emailService;

        public async Task HandleAsync(DocumentEmailRequestedEvent @event, CancellationToken ct = default)
        {
            var req = new CreateEmailRequest
            {
                EntityType = @event.EntityType,
                EntityId = @event.DocumentId,
                Subject = @event.Subject,
                Body = @event.Body,
                Direction = @event.Direction,
                Participants = @event.Participants
                .Select(p => new EmailParticipantResponse
                {
                    DisplayName = p.DisplayName,
                    EmailAddress = p.EmailAddress,
                    ParticipantId = p.ParticipantId,
                    ParticipantType = p.ParticipantType,
                    Role = p.Role
                }).ToList()
            };
            await _emailService.CreateAsync(req, ct);

    }
                
    }
}
