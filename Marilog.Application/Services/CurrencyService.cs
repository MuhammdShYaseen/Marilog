using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IRepository<Currency> _repo;

        public CurrencyService(IRepository<Currency> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Currency?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<Currency?> GetByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .FirstOrDefaultAsync(x => x.CurrencyCode == code.ToUpperInvariant(), ct);

        public async Task<Currency?> GetBaseCurrencyAsync(CancellationToken ct = default)
            => await _repo.Query()
                          .FirstOrDefaultAsync(x => x.IsBaseCurrency, ct);

        public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .OrderBy(x => x.CurrencyCode)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Currency>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.CurrencyCode)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Currency> CreateAsync(string code, string name, decimal exchangeRate,
            string? symbol = null, CancellationToken ct = default)
        {
            var exists = await _repo.Query()
                .AnyAsync(x => x.CurrencyCode == code.ToUpperInvariant(), ct);
            if (exists)
                throw new InvalidOperationException($"Currency code '{code}' already exists.");

            var currency = Currency.Create(code, name, exchangeRate, symbol);
            await _repo.AddAsync(currency, ct);
            await _repo.SaveChangesAsync(ct);
            return currency;
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
    }
}
