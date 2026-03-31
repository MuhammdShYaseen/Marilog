using Marilog.Application.DTOs.Commands.Port;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
{
    public class PortService : IPortService
    {
        private readonly IRepository<Port> _repo;

        public PortService(IRepository<Port> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<PortResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PortResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var upper = code.ToUpperInvariant();

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.PortCode == upper)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<PortResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.PortName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PortResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.PortName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PortResponse>> GetByCountryAsync(int countryId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.CountryID == countryId && x.IsActive)
                .OrderBy(x => x.PortName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .AnyAsync(x => x.PortCode == code.ToUpperInvariant(), ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<PortResponse> CreateAsync(string portCode, string portName,
            int? countryId = null, CancellationToken ct = default)
        {
            if (await ExistsByCodeAsync(portCode, ct))
                throw new InvalidOperationException($"Port code '{portCode}' already exists.");

            var port = Port.Create(portCode, portName, countryId);
            await _repo.AddAsync(port, ct);
            await _repo.SaveChangesAsync(ct);
            return new PortResponse
            {
                Code = portCode,
                CountryId = countryId,
                IsActive = true,
                CountryName = port.Country!.CountryName ?? "",
                Name = portName
            };
        }

        public async Task<IReadOnlyList<PortResponse>> CreateRangeAsync(
        IEnumerable<CreatePortCommand> commands,
        CancellationToken ct = default)
        {
            var commandList = commands.ToList();
            if (!commandList.Any())
                return Array.Empty<PortResponse>();

            // --- تحقق من التكرار داخل الـ batch ---
            var codeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in commandList)
            {
                if (!codeSet.Add(c.PortCode))
                    throw new InvalidOperationException(
                        $"Duplicate PortCode found in the request: '{c.PortCode}'");
            }

            // --- تحقق من التكرار في DB بـ query واحدة ---
            var codes = codeSet.ToList();
            var existingCodes = await _repo.Query()
                .Where(p => codes.Contains(p.PortCode))
                .Select(p => p.PortCode)
                .ToListAsync(ct);

            if (existingCodes.Any())
                throw new InvalidOperationException(
                    $"Port code(s) already exist: {string.Join(", ", existingCodes)}");

            // --- إنشاء دفعة واحدة ---
            var ports = commandList
                .Select(c => Port.Create(c.PortCode, c.PortName, c.CountryId))
                .ToList();

            await _repo.AddRangeAsync(ports, ct);
            await _repo.SaveChangesAsync(ct);

            return ports
                .Select(port => new PortResponse
                {
                    Code = port.PortCode,
                    Name = port.PortName,
                    CountryId = port.CountryID,
                    CountryName = port.Country?.CountryName ?? "",
                    IsActive = port.IsActive
                })
                .ToList();
        }

        public async Task UpdateAsync(int id, string portCode, string portName,
            int? countryId = null, CancellationToken ct = default)
        {
            var port = await GetOrThrowAsync(id, ct);

            var codeConflict = await _repo.Query()
                .AnyAsync(x => x.PortCode == portCode.ToUpperInvariant()
                            && x.Id != id, ct);
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

        private static readonly Expression<Func<Port, PortResponse>> ToResponse =
            x => new PortResponse
        {
           Id = x.Id,
           Code = x.PortCode,
           Name = x.PortName,
           CountryId = x.CountryID,
           CountryName = x.Country != null ? x.Country.CountryName : null,
           IsActive = x.IsActive
        };
    }
}
