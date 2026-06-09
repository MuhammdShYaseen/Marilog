// ═══════════════════════════════════════════════════════════════════════════════
// Controller
// ═══════════════════════════════════════════════════════════════════════════════

using Marilog.Contracts.DTOs.Requests.BillOfLadingDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillOfLadingController : ControllerBase
    {
        private readonly IBillOfLadingService _service;

        public BillOfLadingController(IBillOfLadingService service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => Ok(await _service.GetByIdAsync(id, ct));

        [HttpGet("voyage/{voyageId:int}")]
        public async Task<IActionResult> GetByVoyage(int voyageId, CancellationToken ct)
            => Ok(await _service.GetByVoyageAsync(voyageId, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBillOfLadingRequest request, CancellationToken ct)
            => Ok(await _service.CreateAsync(request, ct));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBillOfLadingRequest request, CancellationToken ct)
            => Ok(await _service.UpdateAsync(id, request, ct));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        // ── Sensitive Operations ─────────────────────────────────────────────
        [HttpPatch("{id:int}/bl-number")]
        public async Task<IActionResult> ChangeBlNumber(int id, [FromBody] ChangeBlNumberRequest request, CancellationToken ct)
            => Ok(await _service.ChangeBlNumberAsync(id, request, ct));

        [HttpPatch("{id:int}/issuance-type")]
        public async Task<IActionResult> ChangeIssuanceType(int id, [FromBody] ChangeIssuanceTypeRequest request, CancellationToken ct)
            => Ok(await _service.ChangeIssuanceTypeAsync(id, request, ct));

        [HttpPatch("{id:int}/link-master-bl")]
        public async Task<IActionResult> LinkToMasterBl(int id, [FromBody] LinkToMasterBlRequest request, CancellationToken ct)
            => Ok(await _service.LinkToMasterBlAsync(id, request, ct));
    }
}