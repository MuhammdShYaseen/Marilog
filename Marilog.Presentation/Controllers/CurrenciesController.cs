namespace Marilog.Presentation.Controllers
{
    using Marilog.Application.DTOs.Commands.Currency;
    using Marilog.Application.DTOs.Responses;
    using Marilog.Application.Interfaces.Services;
    using Marilog.Domain.Entities;
    using Marilog.Presentation.Common;
    using Marilog.Presentation.DTOs.CurrencyDTOs;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/currencies")]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _service;

        public CurrenciesController(ICurrencyService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CurrencyResponse>> GetById(int id, CancellationToken ct)
        {
            var currency = await _service.GetByIdAsync(id, ct);
            return currency is null ? NotFound() : Ok(ApiResponse<CurrencyResponse>.Ok(currency));
        }

        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<CurrencyResponse>> GetByCode(string code, CancellationToken ct)
        {
            var currency = await _service.GetByCodeAsync(code, ct);
            return currency is null ? NotFound() : Ok(ApiResponse<CurrencyResponse>.Ok(currency));
        }

        [HttpGet("base")]
        public async Task<ActionResult<CurrencyResponse>> GetBase(CancellationToken ct)
        {
            var currency = await _service.GetBaseCurrencyAsync(ct);
            return currency is null
                ? throw new KeyNotFoundException("currency not found")
                
                : Ok(ApiResponse<CurrencyResponse>.Ok(new CurrencyResponse
                {
                    Id = currency.Id,
                    Code = currency.CurrencyCode,
                    Name = currency.CurrencyName,
                    Symbol = currency.Symbol,
                    ExchangeRate = currency.ExchangeRate,
                    IsBaseCurrency = currency.IsBaseCurrency,
                    IsActive = currency.IsActive
                }));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CurrencyResponse>>> GetAll(CancellationToken ct)
        {
            var currencies = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<CurrencyResponse>>.Ok(currencies));
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<CurrencyResponse>>> GetActive(CancellationToken ct)
        {
            var currencies = await _service.GetActiveAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<CurrencyResponse>>.Ok(currencies));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<CurrencyResponse>> Create(
            [FromBody] CreateCurrencyRequest request,
            CancellationToken ct)
        {
            var currency = await _service.CreateAsync(
                request.Code,
                request.Name,
                request.ExchangeRate,
                request.Symbol,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = currency.Id }, ApiResponse<CurrencyResponse>.Ok(currency));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CurrencyResponse>>>> CreateRange(
        [FromBody] IEnumerable<CreateCurrencyRequest> requests,
        CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateCurrencyCommand(
                r.Code,
                r.Name,
                r.ExchangeRate,
                r.Symbol
            ));

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<CurrencyResponse>>.Ok(result));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCurrencyRequest request,
            CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.Name, request.ExchangeRate, request.Symbol, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/rate")]
        public async Task<IActionResult> UpdateRate(
            int id,
            [FromBody] UpdateCurrencyRateRequest request,
            CancellationToken ct)
        {
            await _service.UpdateRateAsync(id, request.NewRate, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/set-base")]
        public async Task<IActionResult> SetAsBase(int id, CancellationToken ct)
        {
            await _service.SetAsBaseAsync(id, ct);
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
