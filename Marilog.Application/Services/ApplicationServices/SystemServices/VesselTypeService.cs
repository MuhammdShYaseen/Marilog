using Marilog.Contracts.DTOs.Requests.VesselTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class VesselTypeService : IVesselTypeService
    {
        private readonly IRepository<VesselType> _repo;
        private readonly IRepository<Vessel> _vessels;

        public VesselTypeService(IRepository<VesselType> repo, IRepository<Vessel> vessels)
        {
            _repo = repo;
            _vessels = vessels;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<VesselTypeResponse>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<VesselTypeResponse>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);

        public async Task<VesselTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<VesselTypeResponse> CreateAsync(CreateVesselTypeRequest request, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Code);
            var upper = request.Code.Trim().ToUpperInvariant();

            if (await _repo.Query().AnyAsync(x => x.Code == upper, ct))
                throw new InvalidOperationException($"Vessel type code '{upper}' already exists.");

            var vesselType = VesselType.Create(request.Code, request.Name, request.SortOrder);
            await _repo.AddAsync(vesselType, ct);
            await _repo.SaveChangesAsync(ct);

            return new VesselTypeResponse
            {
                Id = vesselType.Id,
                Code = vesselType.Code,
                Name = vesselType.Name,
                SortOrder = vesselType.SortOrder,
                IsActive = vesselType.IsActive
            };
        }

        public async Task UpdateAsync(int id, UpdateVesselTypeRequest request, CancellationToken ct = default)
        {
            var vesselType = await GetOrThrowAsync(id, ct);
            vesselType.Update(request.Name, request.SortOrder);
            _repo.Update(vesselType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var vesselType = await GetOrThrowAsync(id, ct);
            vesselType.Activate();
            _repo.Update(vesselType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var vesselType = await GetOrThrowAsync(id, ct);
            vesselType.Deactivate();
            _repo.Update(vesselType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var vesselType = await GetOrThrowAsync(id, ct);

            if (await _vessels.Query().AnyAsync(x => x.VesselTypeId == id, ct))
                throw new InvalidOperationException(
                    $"Vessel type '{vesselType.Name}' is used by existing vessels. Deactivate it instead.");

            _repo.HardDelete(vesselType);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<VesselType> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Vessel type {id} not found.");

        private static readonly Expression<Func<VesselType, VesselTypeResponse>> ToResponse =
            x => new VesselTypeResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            };
    }
}