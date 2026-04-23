using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.LaytimeControllers
{
    [ApiController]
    [Route("api/charter-terms")]
    public sealed class CharterTermsController : ControllerBase
    {
        private readonly ICharterTermsService _service;

        public CharterTermsController(ICharterTermsService service)
            => _service = service;

        [HttpPost]
        public async Task<IActionResult> Initialize([FromBody] InitializeCharterTermsRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.InitializeCharterTermsAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { contractId = result.ContractId }, result);
        }

        [HttpGet("{contractId:int}")]
        public async Task<IActionResult> Get(int contractId, CancellationToken cancellationToken)
        {
            var result = await _service.GetCharterTermsAsync(contractId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{contractId:int}/loading")]
        public async Task<IActionResult> UpdateLoading(int contractId,
            [FromBody] CargoOperationTermsRequest request,
            CancellationToken cancellationToken)
        {
            await _service.UpdateLoadingTermsAsync(contractId, request, cancellationToken);
            return NoContent();
        }

        [HttpPut("{contractId:int}/discharging")]
        public async Task<IActionResult> UpdateDischarging(int contractId, [FromBody] CargoOperationTermsRequest request, CancellationToken cancellationToken)
        {
            await _service.UpdateDischargingTermsAsync(contractId, request, cancellationToken);
            return NoContent();
        }

        [HttpPut("{contractId:int}/demurrage")]
        public async Task<IActionResult> UpdateDemurrage(int contractId, [FromBody] DemurrageTermsRequest request, CancellationToken cancellationToken)
        {
            await _service.UpdateDemurrageTermsAsync(contractId, request, cancellationToken);
            return NoContent();
        }

        [HttpPut("{contractId:int}/despatch")]
        public async Task<IActionResult> UpdateDespatch(int contractId, [FromBody] DespatchTermsRequest request, CancellationToken cancellationToken)
        {
            await _service.UpdateDespatchTermsAsync(contractId, request, cancellationToken);
            return NoContent();
        }

        [HttpPut("{contractId:int}/rule-options")]
        public async Task<IActionResult> UpdateRuleOptions(int contractId, [FromBody] LaytimeRuleOptionsRequest request, CancellationToken cancellationToken)
        {
            await _service.UpdateRuleOptionsAsync(contractId, request, cancellationToken);
            return NoContent();
        }
    }
}
