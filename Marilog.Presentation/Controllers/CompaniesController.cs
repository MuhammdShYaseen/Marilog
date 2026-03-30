namespace Marilog.Presentation.Controllers
{
    using Marilog.Application.DTOs.Responses;
    using Marilog.Application.Interfaces.Services;
    using Marilog.Domain.Entities;
    using Marilog.Presentation.Common;
    using Marilog.Presentation.DTOs.CompanyDTOs;
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
                request.CompanyName,
                request.CountryId,
                request.ContactName,
                request.Email,
                request.Phone,
                request.Address,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = company.Id }, ApiResponse<CompanyResponse>.Ok(company));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
        {
            

            await  _service.UpdateAsync(
                id,
                request.CompanyName,
                request.CountryId,
                request.ContactName,
                request.Email,
                request.Phone,
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
    }
}
