using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Presentation.Common;
using Marilog.Presentation.DTOs.VoyageDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoyagesController : ControllerBase
    {
        private readonly IVoyageService _service;

        public VoyagesController(IVoyageService service)
        {
            _service = service;
        }

        // ── Queries ──────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VoyageResponse>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("{id:int}/with-stops")]
        public async Task<ActionResult<VoyageResponse>> GetWithStops(int id, CancellationToken ct)
        {
            var result = await _service.GetWithStopsAsync(id, ct);
            return result is null ? NotFound() : Ok(ApiResponse<VoyageResponse>.Ok(result));
        }

        [HttpGet("by-vessel/{vesselId:int}")]
        public async Task<ActionResult<IReadOnlyList<VoyageResponse>>> GetByVessel(int vesselId, CancellationToken ct)
        {
            return Ok(await _service.GetByVesselAsync(vesselId, ct));
        }

        [HttpGet("by-month")]
        public async Task<ActionResult<IReadOnlyList<VoyageResponse>>> GetByMonth([FromQuery] DateOnly month, CancellationToken ct)
        {
            return Ok(ApiResponse<IReadOnlyList<VoyageResponse>>.Ok(await _service.GetByMonthAsync(month, ct)));
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<IReadOnlyList<VoyageResponse>>> GetByStatus([FromQuery] VoyageStatus status, CancellationToken ct)
        {
            return Ok(ApiResponse<IReadOnlyList<VoyageResponse>>.Ok(await _service.GetByStatusAsync(status, ct)));
        }

        [HttpGet("current/{vesselId:int}")]
        public async Task<ActionResult<VoyageResponse>> GetCurrent(int vesselId, CancellationToken ct)
        {
            var result = await _service.GetCurrentVoyageAsync(vesselId, ct);
            return result is null ? NotFound() : Ok(ApiResponse<VoyageResponse>.Ok(result));
        }

        // ── Commands ────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<VoyageResponse>> Create(CreateVoyageRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(
                request.VesselId,
                request.VoyageNumber,
                request.VoyageMonth,
                request.MasterContractId,
                request.DeparturePortId,
                request.ArrivalPortId,
                request.DepartureDate,
                request.ArrivalDate,
                request.CargoType,
                request.CargoQuantityMt,
                request.Notes,
                ct
            );

            return CreatedAtAction(nameof(GetById), new { id = result.VoyageId }, ApiResponse<VoyageResponse>.Ok(result));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateVoyageRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(
                id,
                request.DeparturePortId,
                request.ArrivalPortId,
                request.DepartureDate,
                request.ArrivalDate,
                request.CargoType,
                request.CargoQuantityMt,
                request.Notes,
                ct
            );

            return NoContent();
        }

        [HttpPut("{id:int}/financials")]
        public async Task<IActionResult> UpdateFinancials(int id, UpdateVoyageFinancialsRequest request, CancellationToken ct)
        {
            await _service.UpdateFinancialsAsync(
                id,
                request.CashOnBoard,
                request.CigarettesOnBoard,
                request.PreviousMasterBalance,
                ct
            );

            return NoContent();
        }

        [HttpPost("{id:int}/assign-master")]
        public async Task<IActionResult> AssignMaster(int id, AssignVoyageMasterRequest request, CancellationToken ct)
        {
            await _service.AssignMasterAsync(id, request.ContractId, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/start")]
        public async Task<IActionResult> Start(int id, CancellationToken ct)
        {
            await _service.StartAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id, CancellationToken ct)
        {
            await _service.CompleteAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            await _service.CancelAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        // ── Stops ───────────────────────────────────────────────

        [HttpPost("{voyageId:int}/stops")]
        public async Task<IActionResult> AddStop(int voyageId, AddVoyageStopRequest request, CancellationToken ct)
        {
            var stop = await _service.AddStopAsync(
                voyageId,
                request.PortId,
                request.StopOrder,
                request.ArrivalDate,
                request.DepartureDate,
                request.PurposeOfCall,
                request.Notes,
                ct
            );

            return Ok(ApiResponse<VoyageStopResponse>.Ok(stop));
        }

        [HttpPut("{voyageId:int}/stops/{stopOrder:int}")]
        public async Task<IActionResult> UpdateStop(int voyageId, int stopOrder, UpdateVoyageStopRequest request, CancellationToken ct)
        {
            await _service.UpdateStopAsync(
                voyageId,
                stopOrder,
                request.ArrivalDate,
                request.DepartureDate,
                request.PurposeOfCall,
                request.Notes,
                ct
            );

            return NoContent();
        }

        [HttpDelete("{voyageId:int}/stops/{stopOrder:int}")]
        public async Task<IActionResult> RemoveStop(int voyageId, int stopOrder, CancellationToken ct)
        {
            await _service.RemoveStopAsync(voyageId, stopOrder, ct);
            return NoContent();
        }
    }
}
