using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.ContractLookupDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/contract-types")]
    public class ContractTypesController : ControllerBase
    {
        private readonly IContractTypeService _service;

        public ContractTypesController(IContractTypeService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<ContractLookupResponse>>>> GetAll(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<ContractLookupResponse>>.Ok(await _service.GetAllAsync(ct)));

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<ContractLookupResponse>>>> GetActive(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<ContractLookupResponse>>.Ok(await _service.GetActiveAsync(ct)));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<ContractLookupResponse>>> GetById(int id, CancellationToken ct)
        {
            var type = await _service.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Contract type {id} not found.");
            return Ok(ApiResponse<ContractLookupResponse>.Ok(type));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContractLookupResponse>>> Create([FromBody] CreateContractLookupRequest request, CancellationToken ct)
            => Ok(ApiResponse<ContractLookupResponse>.Ok(await _service.CreateAsync(request.Code, request.Name, request.SortOrder, ct)));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateContractLookupRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.Name, request.SortOrder, ct);
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
    }
}
