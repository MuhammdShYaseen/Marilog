using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailAccountsController : ControllerBase
    {
        private readonly IEmailAccountService _service;

        public EmailAccountsController(IEmailAccountService service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmailAccountResponse>> GetById(int id, CancellationToken ct)
        {
            var account = await _service.GetByIdAsync(id, ct);
            return account is null ? NotFound() : Ok(ApiResponse<EmailAccountResponse>.Ok(account));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<EmailAccountResponse>>> GetAll(CancellationToken ct)
        {
            var accounts = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<EmailAccountResponse>>.Ok(accounts));
        }

        [HttpPost]
        public async Task<ActionResult<EmailAccountResponse>> Create([FromBody] CreateEmailAccountRequest request, CancellationToken ct)
        {
            var account = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, ApiResponse<EmailAccountResponse>.Ok(account));
        }

        [HttpPatch("{id:int}/rename")]
        public async Task<IActionResult> Rename(int id, [FromBody] RenameEmailAccountRequest request, CancellationToken ct)
        {
            await _service.RenameAsync(id, request.DisplayName, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/config")]
        public async Task<IActionResult> UpdateConfig(
            int id,
            [FromBody] UpdateEmailAccountConfigRequest request,
            CancellationToken ct)
        {
            await _service.UpdateConfigAsync(id, request.Config, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        // Deliberately no endpoint for GetDecryptedConfigAsync — internal use
        // (MailWorker / IEmailProviderClient) only, never exposed over HTTP.
    }
}
