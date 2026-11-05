using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.Contract;
using Marilog.Contracts.DTOs.Requests.ContractDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/[controller]")]
    [PermissionEntity(EntityType.AContract)]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _service;

        public ContractsController(IContractService service)
        {
            _service = service;
        }

        // ─── Read ────────────────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<ContractDetailResponse>>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null)
                throw new KeyNotFoundException("contract not found");
            return Ok(ApiResponse<ContractDetailResponse>.Ok(result));
        }

        [HttpGet("by-number/{number}")]
        public async Task<ActionResult<ApiResponse<ContractDetailResponse>>> GetByNumber(string number, CancellationToken ct)
        {
            var result = await _service.GetByNumberAsync(number, ct);
            if (result == null)
                throw new KeyNotFoundException("contract not found");
            return Ok(ApiResponse<ContractDetailResponse>.Ok(result));
        }

        [RequirePermission(EntityType.AContract, PermissionAction.Read)]
        [HttpPost("paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ContractSummary>>>> GetPaged([FromBody] ContractFilter filter, CancellationToken ct)
        {
            var result = await _service.GetPagedAsync(filter, ct);
            if (result == null)
                throw new KeyNotFoundException("contract not found");
            return Ok(ApiResponse<PagedResponse<ContractSummary>>.Ok(result));
        }

        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResponse<List<ContractSummary>>>> GetExpiring([FromQuery] int withinDays, CancellationToken ct)
        {
            var result = await _service.GetExpiringAsync(withinDays, ct);
            if (result == null)
                throw new KeyNotFoundException("contract not found");
            return Ok(ApiResponse<List<ContractSummary>>.Ok(result));
        }

        [HttpGet("report")]
        public async Task<ActionResult<ApiResponse<ContractReport>>> GetReport(CancellationToken ct)
        {
            var result = await _service.GetReportAsync(ct);
            if (result == null)
                throw new KeyNotFoundException("contract not found");
            return Ok(ApiResponse<ContractReport>.Ok(result));
        }

        // ─── Write ────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Result>>> Create([FromBody] CreateContractRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request.ContractNumber, request.ContractTypeId, request.EffectiveDate, request.ExpiryDate, request.Notes, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpPut("{id:int}/update")]
        public async Task<ActionResult<ApiResponse<Result>>> UpdateAsync(int id, [FromBody] UpdateContractRequest request, CancellationToken ct)
        {
            if (id != request.Id)
                return BadRequest(new ApiResponse<Result>
                {
                    Success = false,
                    Message = "Route id does not match request id."
                });

            var result = await _service.UpdateAsync(
                request.Id,
                request.ContractNumber,
                request.ContractTypeId,
                request.EffectiveDate,
                request.ExpiryDate,
                request.Notes,
                ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<Result>>> Delete(int id, CancellationToken ct)
        {
            var result = await _service.DeleteAsync(id, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpPost("{id}/activate")]
        public async Task<ActionResult<ApiResponse<Result>>> Activate(int id, CancellationToken ct)
        {
            var result = await _service.ActivateAsync(id, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpPost("{id}/suspend")]
        public async Task<ActionResult<ApiResponse<Result>>> Suspend(int id, [FromBody] string reason, CancellationToken ct)
        {
            var result = await _service.SuspendAsync(id, reason, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpPost("{id}/terminate")]
        public async Task<ActionResult<ApiResponse<Result>>> Terminate(int id, [FromBody] string reason, CancellationToken ct)
        {
            var result = await _service.TerminateAsync(id, reason, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpPost("{id}/mark-expired")]
        public async Task<ActionResult<ApiResponse<Result>>> MarkExpired(int id, CancellationToken ct)
        {
            var result = await _service.MarkExpiredAsync(id, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpPost("{id}/party")]
        public async Task<ActionResult<ApiResponse<Result>>> AddParty(int id, [FromBody] PartyRequest request, CancellationToken ct)
        {
            var result = await _service.AddPartyAsync(id, request.CompanyId, request.RoleId, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpDelete("{id}/party")]
        public async Task<ActionResult<ApiResponse<Result>>> RemoveParty(int id, [FromBody] PartyRequest request, CancellationToken ct)
        {
            var result = await _service.RemovePartyAsync(id, request.CompanyId, request.RoleId, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }


        [HttpPost("{id}/amendment")]
        public async Task<ActionResult<ApiResponse<Result>>> RecordAmendment(int id, [FromBody] AmendmentRequest request, CancellationToken ct)
        {
            var result = await _service.RecordAmendmentAsync(id, request.Description, request.EffectiveDate, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

        [HttpPost("{id}/change-expiry")]
        public async Task<ActionResult<ApiResponse<Result>>> ChangeExpiry(int id, [FromBody] ChangeExpiryRequest request, CancellationToken ct)
        {
            var result = await _service.ChangeExpiryAsync(id, request.NewExpiryDate, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error);

            return Ok(ApiResponse<Result>.Ok(result));
        }

    }
}
