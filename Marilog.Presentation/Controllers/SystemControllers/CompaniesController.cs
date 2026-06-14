namespace Marilog.Presentation.Controllers.SystemControllers
{
    using Marilog.Contracts.Common;
    using Marilog.Contracts.DTOs.Requests.CompanyDTOs;
    using Marilog.Contracts.DTOs.Requests.ContactsRequestDTOs;
    using Marilog.Contracts.DTOs.Responses;
    using Marilog.Contracts.Interfaces.Services.SystemServices;
    using Marilog.Kernel.Enums;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _service;

        public CompaniesController(ICompanyService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CompanyResponse>> GetById(int id, CancellationToken ct)
        {
            var company = await _service.GetByIdAsync(id, ct);
            if (company == null)
                throw new KeyNotFoundException("Company not found");

            return Ok(ApiResponse<CompanyResponse>.Ok(company));
        }

        [HttpGet("{id:int}/with-vessels")]
        public async Task<ActionResult<CompanyResponse>> GetWithVessels(int id, CancellationToken ct)
        {
            var company = await _service.GetWithVesselsAsync(id, ct);
            if (company == null)
                throw new KeyNotFoundException("Company not found");

            return Ok(ApiResponse<CompanyResponse>.Ok(company));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> GetAll(CancellationToken ct)
        {
            var companies = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<CompanyResponse>>.Ok(companies));
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> GetActive(CancellationToken ct)
        {
            var companies = await _service.GetActiveAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<CompanyResponse>>.Ok(companies));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> Search([FromQuery] string name, CancellationToken ct)
        {
            var companies = await _service.SearchByNameAsync(name, ct);
            return Ok(ApiResponse<IReadOnlyList<CompanyResponse>>.Ok(companies));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
        {
            var company = await _service.CreateAsync(
                request.RegistrationNumber,
                request.WebSite,
                request.CompanyName,
                request.CountryId,
                request.ContactName,
                request.Address,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = company.Id }, ApiResponse<CompanyResponse>.Ok(company));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> CreateRange(
        [FromBody] IEnumerable<CreateCompanyRequest> requests,
        CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateCompanyRequest
            {
                RegistrationNumber = r.RegistrationNumber,
                WebSite = r.WebSite,
                CompanyName = r.CompanyName,
                CountryId = r.CountryId,
                ContactName = r.ContactName,
                Address = r.Address
            }
            );

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
        {
            

            await  _service.UpdateAsync(
                id,
                request.RegistrationNumber,
                request.WebSite,
                request.CompanyName,
                request.CountryId,
                request.ContactName,
                request.Address,
                ct);

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



        // ─────────────────────────────────────────────
        // Bank Accounts
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/bank-accounts")]
        public async Task<IActionResult> AddBankAccount(int id, [FromBody] AddBankAccountRequest request, CancellationToken ct)
        {
            await _service.AddBankAccountAsync(id, request.IBAN, request.BankName, request.SwiftCode,
                request.CurrencyId, request.AccountHolderName, request.IsPrimary, ct);
            return NoContent();
        }

        [HttpPut("{id:int}/bank-accounts/{iban}")]
        public async Task<IActionResult> UpdateBankAccount(int id, string iban, [FromBody] AddBankAccountRequest request, CancellationToken ct)
        {
            await _service.UpdateBankAccountAsync(id, iban, request.BankName, request.SwiftCode,
                request.CurrencyId, request.AccountHolderName, request.IsPrimary, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}/bank-accounts/{iban}")]
        public async Task<IActionResult> RemoveBankAccount(int id, string iban, CancellationToken ct)
        {
            await _service.RemoveBankAccountAsync(id, iban, ct);
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // Emails
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/emails")]
        public async Task<IActionResult> AddEmail(int id, [FromBody] AddEmailRequest request, CancellationToken ct)
        {
            await _service.AddEmailAsync(id, request.Address, request.Role, request.Label, request.IsPrimary, ct);
            return NoContent();
        }

        [HttpPut("{id:int}/emails/{address}")]
        public async Task<IActionResult> UpdateEmail(int id, string address, [FromBody] AddEmailRequest request, CancellationToken ct)
        {
            await _service.UpdateEmailAsync(id, address, request.Address, request.Role, request.Label, request.IsPrimary, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}/emails/{address}")]
        public async Task<IActionResult> RemoveEmail(int id, string address, CancellationToken ct)
        {
            await _service.RemoveEmailAsync(id, address, ct);
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // Phones
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/phones")]
        public async Task<IActionResult> AddPhone(int id, [FromBody] AddPhoneRequest request, CancellationToken ct)
        {
            await _service.AddPhoneAsync(id, request.Number, request.Type, request.Label, request.IsPrimary, ct);
            return NoContent();
        }

        [HttpPut("{id:int}/phones")]
        public async Task<IActionResult> UpdatePhone(int id, [FromBody] UpdatePhoneRequest request, CancellationToken ct)
        {
            await _service.UpdatePhoneAsync(id, request.OldNumber, request.OldType,
                request.NewNumber, request.NewType, request.Label, request.IsPrimary, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}/phones")]
        public async Task<IActionResult> RemovePhone(int id, [FromQuery] string number, [FromQuery] PhoneType type, CancellationToken ct)
        {
            await _service.RemovePhoneAsync(id, number, type, ct);
            return NoContent();
        }
    }
}
