using Marilog.Application.DTOs.Commands.Currency;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IRepository<Currency> _repo;

        public CurrencyService(IRepository<Currency> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CurrencyResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<CurrencyResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var upper = code.ToUpperInvariant();

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.CurrencyCode == upper)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Currency?> GetBaseCurrencyAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsBaseCurrency)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<CurrencyResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.CurrencyCode)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CurrencyResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.CurrencyCode)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CurrencyResponse> CreateAsync(string code, string name, decimal exchangeRate,
            string? symbol = null, CancellationToken ct = default)
        {
            var exists = await _repo.Query()
                .AnyAsync(x => x.CurrencyCode == code.ToUpperInvariant(), ct);
            if (exists)
                throw new InvalidOperationException($"Currency code '{code}' already exists.");

            var currency = Currency.Create(code, name, exchangeRate, symbol);
            await _repo.AddAsync(currency, ct);
            await _repo.SaveChangesAsync(ct);
            return new CurrencyResponse
            {
                Id = currency.Id,
                Code = currency.CurrencyCode,
                Name = currency.CurrencyName,
                Symbol = currency.Symbol,
                ExchangeRate = currency.ExchangeRate,
                IsBaseCurrency = currency.IsBaseCurrency,
                IsActive = currency.IsActive
            };
        }

        public async Task<IReadOnlyList<CurrencyResponse>> CreateRangeAsync(
        IEnumerable<CreateCurrencyCommand> commands,
        CancellationToken ct = default)
        {
            var codes = commands.Select(c => c.Code.ToUpperInvariant()).ToList();

            var existingCodes = await _repo.Query()
                .Where(x => codes.Contains(x.CurrencyCode))
                .Select(x => x.CurrencyCode)
                .ToListAsync(ct);

            if (existingCodes.Any())
                throw new InvalidOperationException(
                    $"Currency codes already exist: {string.Join(", ", existingCodes)}");

            var currencies = commands
                .Select(c => Currency.Create(c.Code, c.Name, c.ExchangeRate, c.Symbol))
                .ToList();

            await _repo.AddRangeAsync(currencies, ct);
            await _repo.SaveChangesAsync(ct);

            return currencies
                .Select(currency => new CurrencyResponse
                {
                    Id = currency.Id,
                    Code = currency.CurrencyCode,
                    Name = currency.CurrencyName,
                    Symbol = currency.Symbol,
                    ExchangeRate = currency.ExchangeRate,
                    IsBaseCurrency = currency.IsBaseCurrency,
                    IsActive = currency.IsActive
                })
                .ToList();
        }
        public async Task UpdateAsync(int id, string name, decimal exchangeRate,
            string? symbol = null, CancellationToken ct = default)
        {
            var currency = await GetOrThrowAsync(id, ct);
            currency.Update(name, exchangeRate, symbol);
            _repo.Update(currency);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateRateAsync(int id, decimal newRate, CancellationToken ct = default)
        {
            var currency = await GetOrThrowAsync(id, ct);
            currency.UpdateRate(newRate);
            _repo.Update(currency);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetAsBaseAsync(int id, CancellationToken ct = default)
        {
            // Unset current base currency first
            var currentBase = await GetBaseCurrencyAsync(ct);
            if (currentBase is not null && currentBase.Id != id)
            {
                currentBase.Update(currentBase.CurrencyName, currentBase.ExchangeRate, currentBase.Symbol);
                _repo.Update(currentBase);
            }

            var currency = await GetOrThrowAsync(id, ct);
            currency.SetAsBase();
            _repo.Update(currency);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var currency = await GetOrThrowAsync(id, ct);
            currency.Activate();
            _repo.Update(currency);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var currency = await GetOrThrowAsync(id, ct);
            if (currency.IsBaseCurrency)
                throw new InvalidOperationException("Cannot deactivate the base currency.");
            currency.Deactivate();
            _repo.Update(currency);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var currency = await GetOrThrowAsync(id, ct);
            if (currency.IsBaseCurrency)
                throw new InvalidOperationException("Cannot delete the base currency.");
            _repo.HardDelete(currency);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Currency> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Currency {id} not found.");

        private static readonly Expression<Func<Currency, CurrencyResponse>> ToResponse =
            x => new CurrencyResponse
            {
                Id = x.Id,
                Code = x.CurrencyCode,
                Name = x.CurrencyName,
                Symbol = x.Symbol,
                ExchangeRate = x.ExchangeRate,
                IsBaseCurrency = x.IsBaseCurrency,
                IsActive = x.IsActive
            };
    }
}
