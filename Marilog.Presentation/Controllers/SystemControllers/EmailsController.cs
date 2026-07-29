using Marilog.Application.Services.ApplicationServices.SystemServices;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.EmailServices;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
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
        public async Task<ActionResult<EmailResponse>> GetById(int id, CancellationToken ct)
        {
            var email = await _service.GetByIdAsync(id, ct);
            return email is null ? NotFound() : Ok(ApiResponse<EmailResponse>.Ok(email));
        }

        [HttpGet("{id:int}/full")]
        public async Task<ActionResult<EmailResponse>> GetFull(int id, CancellationToken ct)
        {
            var email = await _service.GetFullAsync(id, ct);
            return email is null ? NotFound() : Ok(ApiResponse<EmailResponse>.Ok(email));
        }

        [HttpGet("unlinked")]
        public async Task<ActionResult<IReadOnlyList<EmailResponse>>> GetUnlinked(CancellationToken ct)
        {
            var result = await _service.GetUnlinkedAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<EmailResponse>>.Ok(result));
        }

        [HttpGet("by-entity")]
        public async Task<ActionResult<IReadOnlyList<EmailResponse>>> GetByEntity(
            [FromQuery] EntityType entityType,
            [FromQuery] int entityId,
            CancellationToken ct)
        {
            var result = await _service.GetByEntityAsync(entityType, entityId, ct);
            return Ok(ApiResponse < IReadOnlyList<EmailResponse>>.Ok(result));
        }

        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<IReadOnlyList<EmailResponse>>> GetByStatus(
            EmailStatus status,
            CancellationToken ct)
        {
            var results = await _service.GetByStatusAsync(status, ct);
            return Ok(ApiResponse<IReadOnlyList<EmailResponse>>.Ok(results));
        }

        [HttpGet("by-participant")]
        public async Task<ActionResult<IReadOnlyList<EmailResponse>>> GetByParticipant(
            [FromQuery] ParticipantType participantType,
            [FromQuery] int participantId,
            CancellationToken ct)
        {
            var results = await _service.GetByParticipantAsync(participantType, participantId, ct);
            return Ok(ApiResponse<IReadOnlyList<EmailResponse>>.Ok(results));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<EmailResponse>> Create(
            [FromBody] CreateEmailRequest request,
            CancellationToken ct)
        {
            var email = await _service.CreateAsync(request, ct);

            return CreatedAtAction(nameof(GetById), new { id = email.Id }, ApiResponse<EmailResponse>.Ok(email));
        }

        [HttpPut("{emailId:int}/entity")]
        public async Task<ActionResult<EmailResponse>> UpsertEntity(int emailId, [FromBody] UpsertEmailEntityRequest request, CancellationToken ct)
        {
            var result = await _service.RelinkAsync(emailId, request.EntityType,
                request.EntityId, ct);
            return Ok(result);
        }
        [HttpPost("{id:int}/send")]
        public async Task<ActionResult<EmailResponse>> Send(int id, CancellationToken ct)
        {
            var result = await _service.SendEmailAsync(id, ct);
            return Ok(ApiResponse<EmailResponse>.Ok(result));
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
        public async Task<ActionResult<EmailParticipantResponse>> AddParticipant(
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

            return Ok(ApiResponse<EmailParticipantResponse>.Ok(participant));
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

       
    }
}
