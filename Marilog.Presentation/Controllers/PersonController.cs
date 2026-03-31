using Marilog.Application.DTOs.Commands.Person;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Presentation.Common;
using Marilog.Presentation.DTOs.PersonDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _service;

        public PersonController(IPersonService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PersonResponse>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result is null) return NotFound();
            return Ok(ApiResponse<PersonResponse>.Ok(result));
        }

        [HttpGet("passport/{passportNo}")]
        public async Task<ActionResult<PersonResponse>> GetByPassport(string passportNo, CancellationToken ct)
        {
            var result = await _service.GetByPassportAsync(passportNo, ct);
            if (result is null) return NotFound();
            return Ok(ApiResponse<PersonResponse>.Ok(result));
        }

        [HttpGet("seamanbook/{seamanBookNo}")]
        public async Task<ActionResult<PersonResponse>> GetBySeamanBook(string seamanBookNo, CancellationToken ct)
        {
            var result = await _service.GetBySeamanBookAsync(seamanBookNo, ct);
            if (result is null) return NotFound();
            return Ok(ApiResponse<PersonResponse>.Ok(result));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PersonResponse>>> GetAll(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<PersonResponse>>.Ok(await _service.GetAllAsync(ct)));

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<PersonResponse>>> GetActive(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<PersonResponse>>.Ok(await _service.GetActiveAsync(ct)));

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<PersonResponse>>> Search([FromQuery] string term, CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<PersonResponse>>.Ok(await _service.SearchAsync(term, ct)));

        [HttpGet("expiring-passports")]
        public async Task<ActionResult<IReadOnlyList<PersonResponse>>> GetWithExpiringPassports([FromQuery] int withinDays, CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<PersonResponse>>.Ok(await _service.GetWithExpiringPassportsAsync(withinDays, ct)));

        [HttpPost]
        public async Task<ActionResult<PersonResponse>> Create([FromBody] CreatePersonRequest request, CancellationToken ct)
        {
            var response = await _service.CreateAsync(
                request.BankName,
                request.IBAN,
                request.IsPassportExpired,
                request.BankSwiftCode,
                request.FullName,
                request.Nationality,
                request.PassportNo,
                request.PassportExpiry,
                request.SeamanBookNo,
                request.DateOfBirth,
                request.Phone,
                request.Email,
                ct
            );
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, ApiResponse<PersonResponse>.Ok(response));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<PersonResponse>>>> CreateRange(
        [FromBody] IEnumerable<CreatePersonRequest> requests,
        CancellationToken ct)
        {
            var commands = requests.Select(r => new CreatePersonCommand(
                r.BankName,
                r.IBAN,
                r.IsPassportExpired,
                r.FullName,
                r.BankSwiftCode,
                r.Nationality,
                r.PassportNo,
                r.PassportExpiry,
                r.SeamanBookNo,
                r.DateOfBirth,
                r.Phone,
                r.Email
            ));

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<PersonResponse>>.Ok(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePersonRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(
                id,
                request.FullName,
                request.Nationality,
                request.PassportNo,
                request.PassportExpiry,
                request.SeamanBookNo,
                request.DateOfBirth,
                request.Phone,
                request.Email,
                ct
            );
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }

        // ── Bank Account Endpoints ───────────────────────────────────────────────

        [HttpPut("{id}/bank-account")]
        public async Task<IActionResult> UpdateBankAccount(int id, [FromBody] UpdateBankAccountRequest request, CancellationToken ct)
        {
            await _service.UpdateBankAccountAsync(id, request.BankName, request.IBAN, request.BankSwiftCode, ct);
            return NoContent();
        }

        [HttpPost("{id}/bank-account/clear")]
        public async Task<IActionResult> ClearBankAccount(int id, CancellationToken ct)
        {
            await _service.ClearBankAccountAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
