using Marilog.Domain.Events;
using Marilog.Application.Interfaces.Services;

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

        public async Task HandleAsync(DocumentEmailRequestedEvent @event,
            CancellationToken ct = default)
        {
            await _emailService.CreateAsync(
                entityType:   @event.EntityType,
                entityId:     @event.DocumentId,
                subject:      @event.Subject,
                body:         @event.Body,
                direction:    @event.Direction,
                participants: @event.Participants,
                ct:           ct);
        }
    }
}
