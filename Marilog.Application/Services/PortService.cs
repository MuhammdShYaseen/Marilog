using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class PortService : IPortService
    {
        private readonly IRepository<Port> _repo;

        public PortService(IRepository<Port> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Port?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<Port?> GetByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .FirstOrDefaultAsync(x => x.PortCode == code.ToUpperInvariant(), ct);

        public async Task<IReadOnlyList<Port>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .OrderBy(x => x.PortName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Port>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.PortName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Port>> GetByCountryAsync(int countryId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.CountryID == countryId && x.IsActive)
                          .OrderBy(x => x.PortName)
                          .ToListAsync(ct);

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .AnyAsync(x => x.PortCode == code.ToUpperInvariant(), ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Port> CreateAsync(string portCode, string portName,
            int? countryId = null, CancellationToken ct = default)
        {
            if (await ExistsByCodeAsync(portCode, ct))
                throw new InvalidOperationException($"Port code '{portCode}' already exists.");

            var port = Port.Create(portCode, portName, countryId);
            await _repo.AddAsync(port, ct);
            await _repo.SaveChangesAsync(ct);
            return port;
        }

        public async Task UpdateAsync(int id, string portCode, string portName,
            int? countryId = null, CancellationToken ct = default)
        {
            var port = await GetOrThrowAsync(id, ct);

            var codeConflict = await _repo.Query()
                .AnyAsync(x => x.PortCode == portCode.ToUpperInvariant()
                            && x.PortID   != id, ct);
            if (codeConflict)
                throw new InvalidOperationException($"Port code '{portCode}' is already in use.");

            port.Update(portCode, portName, countryId);
            _repo.Update(port);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var port = await GetOrThrowAsync(id, ct);
            port.Activate();
            _repo.Update(port);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var port = await GetOrThrowAsync(id, ct);
            port.Deactivate();
            _repo.Update(port);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var port = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(port);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Port> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Port {id} not found.");
    }
}
