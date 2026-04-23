using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwiftTransfersController : ControllerBase
    {
        private readonly ISwiftTransferService _service;

        public SwiftTransfersController(ISwiftTransferService service)
        {
            _service = service;
        }

        // ── Queries ──────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SwiftTransferResponse>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result is null ? NotFound() : Ok(ApiResponse<SwiftTransferResponse>.Ok(result));
        }

        [HttpGet("by-reference/{reference}")]
        public async Task<ActionResult<SwiftTransferResponse>> GetByReference(string reference, CancellationToken ct)
        {
            var result = await _service.GetByReferenceAsync(reference, ct);
            return result is null ? NotFound() : Ok(ApiResponse<SwiftTransferResponse>.Ok(result));
        }

        [HttpGet("by-sender/{companyId:int}")]
        public async Task<ActionResult<IReadOnlyList<SwiftTransferResponse>>> GetBySender(int companyId, CancellationToken ct)
        {
            var result = await _service.GetBySenderAsync(companyId, ct);
            return Ok(ApiResponse< IReadOnlyList<SwiftTransferResponse>>.Ok(result));
        }

        [HttpGet("by-receiver/{companyId:int}")]
        public async Task<ActionResult<IReadOnlyList<SwiftTransferResponse>>> GetByReceiver(int companyId, CancellationToken ct)
        {
            var result = await _service.GetByReceiverAsync(companyId, ct);
            return Ok(ApiResponse<IReadOnlyList<SwiftTransferResponse>>.Ok(result));
        }

        [HttpGet("unallocated")]
        public async Task<ActionResult<IReadOnlyList<SwiftTransferResponse>>> GetUnallocated(CancellationToken ct)
        {
            var result = await _service.GetUnallocatedAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<SwiftTransferResponse>>.Ok(result));
        }

        [HttpGet("by-date-range")]
        public async Task<ActionResult<IReadOnlyList<SwiftTransferResponse>>> GetByDateRange([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        {
            var result = await _service.GetByDateRangeAsync(from, to, ct);
            return Ok(ApiResponse<IReadOnlyList<SwiftTransferResponse>>.Ok(result));
        }

        // ── Commands ────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<SwiftTransferResponse>> Create(CreateSwiftTransferRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(
                request.SwiftReference,
                request.TransactionDate,
                request.CurrencyId,
                request.Amount,
                request.SenderCompanyId,
                request.ReceiverCompanyId,
                request.SenderBank,
                request.ReceiverBank,
                request.PaymentReference,
                request.RawMessage,
                ct
            );

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SwiftTransferResponse>.Ok(result));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<SwiftTransferResponse>>>> CreateRange(
         [FromBody] IEnumerable<CreateSwiftTransferRequest> requests,
         CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateSwiftTransferRequest{
                SwiftReference = r.SwiftReference,
                TransactionDate = r.TransactionDate,
                CurrencyId = r.CurrencyId,
                Amount = r.Amount,
                SenderCompanyId = r.SenderCompanyId,
                ReceiverCompanyId = r.ReceiverCompanyId,
                SenderBank = r.SenderBank,
                ReceiverBank = r.ReceiverBank,
                PaymentReference = r.PaymentReference,
                RawMessage = r.RawMessage
            });

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<SwiftTransferResponse>>.Ok(result));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateSwiftTransferRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(
                id,
                request.CurrencyId,
                request.Amount,
                request.SenderBank,
                request.ReceiverBank,
                request.PaymentReference,
                request.RawMessage,
                ct
            );

            return NoContent();
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/deactivate")]
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
    }
}
