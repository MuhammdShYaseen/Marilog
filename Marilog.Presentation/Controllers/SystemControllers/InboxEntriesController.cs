using DocumentFormat.OpenXml.Spreadsheet;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.InboxEntryReport;
using Marilog.Contracts.DTOs.Requests.InboxEntryDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/inbox-entries")]
    public class InboxEntriesController : ControllerBase
    {
        private readonly IInboxService _inbox;

        public InboxEntriesController(IInboxService inbox) => _inbox = inbox;

        [HttpPost]
        public async Task<IActionResult> Create(CreateInboxEntryRequest request, CancellationToken ct)
            => Ok(await _inbox.CreateAsync(request.ReceivedDate, request.Source,
                request.VesselIds ?? [], request.Description, request.StartSequence, ct));

        [HttpPost("range")]
        public async Task<IActionResult> CreateRange(List<CreateInboxEntryItem> items,
             [FromQuery] bool keepInputOrder, CancellationToken ct)
                => Ok(await _inbox.CreateRangeAsync(items, keepInputOrder, ct));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateInboxEntryRequest request, CancellationToken ct)
        {
            await _inbox.UpdateAsync(id, request.ReceivedDate, request.Source,
                request.VesselIds ?? [], request.Description, ct);
            return NoContent();
        }

        [HttpPut("{id:int}/start")]
        public async Task<IActionResult> ChangeStart(int id, ChangeInboxStartRequest request, CancellationToken ct)
            => Ok(await _inbox.ChangeStartAsync(id, request.Sequence, ct));

        [HttpPost("{id:int}/links")]
        public async Task<IActionResult> Link(int id, LinkInboxEntryRequest request, CancellationToken ct)
        {
            await _inbox.LinkAsync(id, request.EntityType, request.EntityId, ct);
            return NoContent();
        }

        [HttpDelete("links/{linkId:int}")]
        public async Task<IActionResult> Unlink(int linkId, CancellationToken ct)
        {
            await _inbox.UnlinkAsync(linkId, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/processed")]
        public async Task<IActionResult> MarkProcessed(int id, CancellationToken ct)
        {
            await _inbox.MarkProcessedAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            await _inbox.CancelAsync(id, ct);
            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _inbox.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request, CancellationToken ct)
            => Ok(await _inbox.GetPagedAsync(request, ct));

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] InboxEntryFilter filter, CancellationToken ct)
            => Ok(await _inbox.GetFilteredAsync(filter, ct));


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetbyId(int id, CancellationToken ct)
            => await _inbox.GetByIdAsync(id, ct) is { } entry ? Ok(entry) : NotFound();


    }
}
