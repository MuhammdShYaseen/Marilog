
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.BankDTOs;
using Marilog.Contracts.DTOs.Requests.ContactsRequestDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/banks")]
    public class BanksController : ControllerBase
    {
        private readonly IBankService _bankService;

        public BanksController(IBankService bankService) => _bankService = bankService;

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BankResponse>> GetById(int id, CancellationToken ct)
        {
            var bank = await _bankService.GetByIdAsync(id, ct);
            return bank is null ? NotFound() : Ok(bank);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<BankResponse>>> GetPaged(
            [FromQuery] bool treeMode = false, CancellationToken ct = default)
            => Ok(await _bankService.GetPagedAsync(treeMode, ct));

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<BankResponse>>> GetAllActive(
            [FromQuery] bool treeMode = false, CancellationToken ct = default)
            => Ok(await _bankService.GetAllActiveAsync(treeMode, ct));

        [HttpGet("by-country/{countryId:int}")]
        public async Task<ActionResult<IReadOnlyList<BankResponse>>> GetByCountry(
            int countryId, [FromQuery] bool treeMode = false, CancellationToken ct = default)
            => Ok(await _bankService.GetByCountryIdAsync(countryId, treeMode, ct));

        [HttpGet("lookup")]
        public async Task<ActionResult<List<LookupItem<int>>>> GetLookup(CancellationToken ct)
            => Ok(await _bankService.GetLookupAsync(ct));

        [HttpGet("lookup/parents")]
        public async Task<ActionResult<List<LookupItem<int>>>> GetParentBanksLookup(
            [FromQuery] int? excludeBankId, CancellationToken ct)
            => Ok(await _bankService.GetParentBanksLookupAsync(excludeBankId, ct));

        [HttpPost]
        public async Task<ActionResult<BankResponse>> Create(CreateBankRequest request, CancellationToken ct)
        {
            var created = await _bankService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.BankId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateBankRequest request, CancellationToken ct)
        {
            if (id != request.Id) return BadRequest("Route id and body id mismatch.");
            var result = await _bankService.UpdateAsync(request, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            var result = await _bankService.DeactivateAsync(id, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _bankService.ActivateAsync(id, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _bankService.DeleteAsync(id, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPost("{id:int}/emails")]
        public async Task<IActionResult> AddEmail(int id, AddEmailRequest request, CancellationToken ct)
        {
            var result = await _bankService.AddEmailAsync(id, request, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpDelete("{id:int}/emails/{email}")]
        public async Task<IActionResult> RemoveEmail(int id, string email, CancellationToken ct)
        {
            var result = await _bankService.RemoveEmailAsync(id, email, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPost("{id:int}/phones")]
        public async Task<IActionResult> AddPhone(int id, AddPhoneRequest request, CancellationToken ct)
        {
            var result = await _bankService.AddPhoneAsync(id, request, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpDelete("{id:int}/phones/{phoneNumber}")]
        public async Task<IActionResult> RemovePhone(int id, string phoneNumber, CancellationToken ct)
        {
            var result = await _bankService.RemovePhoneAsync(id, phoneNumber, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
    }
}