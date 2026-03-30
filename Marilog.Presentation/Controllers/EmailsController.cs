using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Presentation.Common;
using Marilog.Presentation.DTOs.EmailDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/emails")]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _service;

        public EmailsController(IEmailService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Email>> GetById(int id, CancellationToken ct)
        {
            var email = await _service.GetByIdAsync(id, ct);
            return email is null ? NotFound() : Ok(ApiResponse<Email>.Ok(email));
        }

        [HttpGet("{id:int}/full")]
        public async Task<ActionResult<Email>> GetFull(int id, CancellationToken ct)
        {
            var email = await _service.GetFullAsync(id, ct);
            return email is null ? NotFound() : Ok(ApiResponse<Email>.Ok(email));
        }

        [HttpGet("by-entity")]
        public async Task<ActionResult<IReadOnlyList<Email>>> GetByEntity(
            [FromQuery] string entityType,
            [FromQuery] int entityId,
            CancellationToken ct)
        {
            var result = await _service.GetByEntityAsync(entityType, entityId, ct);
            return Ok(ApiResponse < IReadOnlyList<Email>>.Ok(result));
        }

        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<IReadOnlyList<Email>>> GetByStatus(
            EmailStatus status,
            CancellationToken ct)
        {
            var results = await _service.GetByStatusAsync(status, ct);
            return Ok(ApiResponse<IReadOnlyList<Email>>.Ok(results));
        }

        [HttpGet("by-participant")]
        public async Task<ActionResult<IReadOnlyList<Email>>> GetByParticipant(
            [FromQuery] ParticipantType participantType,
            [FromQuery] int participantId,
            CancellationToken ct)
        {
            var results = await _service.GetByParticipantAsync(participantType, participantId, ct);
            return Ok(ApiResponse<IReadOnlyList<Email>>.Ok(results));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<Email>> Create(
            [FromBody] CreateEmailRequest request,
            CancellationToken ct)
        {
            var email = await _service.CreateAsync(
                request.EntityType,
                request.EntityId,
                request.Subject,
                request.Body,
                request.Direction,
                request.Participants,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = email.Id }, ApiResponse<Email>.Ok(email));
        }

        [HttpPatch("{id:int}/mark-sent")]
        public async Task<IActionResult> MarkAsSent(
            int id,
            [FromBody] MarkEmailSentRequest request,
            CancellationToken ct)
        {
            await _service.MarkAsSentAsync(id, request.SentAt, request.ExternalId, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/mark-received")]
        public async Task<IActionResult> MarkAsReceived(int id, CancellationToken ct)
        {
            await _service.MarkAsReceivedAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/mark-failed")]
        public async Task<IActionResult> MarkAsFailed(int id, CancellationToken ct)
        {
            await _service.MarkAsFailedAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/retry")]
        public async Task<IActionResult> Retry(int id, CancellationToken ct)
        {
            await _service.RetryAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // Participants
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/participants")]
        public async Task<ActionResult<EmailParticipant>> AddParticipant(
            int id,
            [FromBody] AddParticipantRequest request,
            CancellationToken ct)
        {
            var participant = await _service.AddParticipantAsync(
                id,
                request.Role,
                request.ParticipantType,
                request.ParticipantId,
                request.DisplayName,
                request.EmailAddress,
                ct);

            return Ok(ApiResponse<EmailParticipant>.Ok(participant));
        }

        [HttpDelete("{id:int}/participants/{participantId:int}")]
        public async Task<IActionResult> RemoveParticipant(
            int id,
            int participantId,
            CancellationToken ct)
        {
            await _service.RemoveParticipantAsync(id, participantId, ct);
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // Attachments
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/attachments")]
        public async Task<ActionResult<EmailAttachment>> AddAttachment(
            int id,
            [FromBody] AddAttachmentRequest request,
            CancellationToken ct)
        {
            var attachment = await _service.AddAttachmentAsync(
                id,
                request.FileName,
                request.FilePath,
                request.FileSizeBytes,
                ct);

            return Ok(ApiResponse<EmailAttachment>.Ok(attachment));
        }

        [HttpDelete("{id:int}/attachments/{attachmentId:int}")]
        public async Task<IActionResult> RemoveAttachment(
            int id,
            int attachmentId,
            CancellationToken ct)
        {
            await _service.RemoveAttachmentAsync(id, attachmentId, ct);
            return NoContent();
        }
    }
}
