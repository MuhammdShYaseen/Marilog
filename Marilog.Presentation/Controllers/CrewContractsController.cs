using Marilog.Application.DTOs;
using Marilog.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/crew-contracts")]
    public class CrewContractsController : ControllerBase
    {
        private readonly ICrewContractService _service;

        public CrewContractsController(ICrewContractService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CrewContractResponse>> GetById(int id, CancellationToken ct)
        {
            var contract = await _service.GetByIdAsync(id, ct);
            return contract is null ? NotFound() : Ok(contract);
        }

        [HttpGet("by-person/{personId:int}")]
        public async Task<ActionResult<IReadOnlyList<CrewContractResponse>>> GetByPerson(int personId, CancellationToken ct)
        {
            var contracts = await _service.GetByPersonAsync(personId, ct);
            return Ok(contracts);
        }

        [HttpGet("by-vessel/{vesselId:int}")]
        public async Task<ActionResult<IReadOnlyList<CrewContractResponse>>> GetByVessel(int vesselId, CancellationToken ct)
        {
            var contracts = await _service.GetByVesselAsync(vesselId, ct);
            return Ok(contracts);
        }

        [HttpGet("active/by-vessel/{vesselId:int}")]
        public async Task<ActionResult<IReadOnlyList<CrewContractResponse>>> GetActiveByVessel(int vesselId, CancellationToken ct)
        {
            var contracts = await _service.GetActiveByVesselAsync(vesselId, ct);
            return Ok(contracts);
        }

        [HttpGet("active-master/{vesselId:int}")]
        public async Task<ActionResult<CrewContractResponse>> GetActiveMaster(int vesselId, CancellationToken ct)
        {
            var contract = await _service.GetActiveMasterAsync(vesselId, ct);
            return contract is null ? NotFound() : Ok(contract);
        }

        [HttpGet("active-on-date")]
        public async Task<ActionResult<IReadOnlyList<CrewContractResponse>>> GetActiveOnDate(
            [FromQuery] DateOnly date,
            CancellationToken ct)
        {
            var contracts = await _service.GetActiveOnDateAsync(date, ct);
            return Ok(contracts);
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<CrewContractResponse>> Create(
            [FromBody] CreateCrewContractRequest request,
            CancellationToken ct)
        {
            var contract = await _service.CreateAsync(
                request.PersonId,
                request.VesselId,
                request.RankId,
                request.MonthlyWage,
                request.SignOnDate,
                request.SignOnPort,
                request.Notes,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = contract.ContractId }, contract);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCrewContractRequest request,
            CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.MonthlyWage, request.Notes, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/sign-off")]
        public async Task<IActionResult> SignOff(
            int id,
            [FromBody] SignOffCrewContractRequest request,
            CancellationToken ct)
        {
            await _service.SignOffAsync(id, request.SignOffDate, request.SignOffPort, ct);
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
