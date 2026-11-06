using Marilog.Contracts.DTOs.Requests.CargoTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class CargoTypeService : ICargoTypeService
    {
        private readonly IRepository<CargoType> _repo;

        public CargoTypeService(IRepository<CargoType> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<CargoTypeResponse>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<CargoTypeResponse>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);

        public async Task<CargoTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CargoTypeResponse> CreateAsync(CreateCargoTypeRequest request, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Code);
            var upper = request.Code.Trim().ToUpperInvariant();

            if (await _repo.Query().AnyAsync(x => x.Code == upper, ct))
                throw new InvalidOperationException($"Cargo type code '{upper}' already exists.");

            var cargoType = CargoType.Create(request.Code, request.Name, request.Category,
                request.DefaultUnit, request.SortOrder);

            await _repo.AddAsync(cargoType, ct);
            await _repo.SaveChangesAsync(ct);

            return new CargoTypeResponse
            {
                Id = cargoType.Id,
                Code = cargoType.Code,
                Name = cargoType.Name,
                Category = cargoType.Category,
                DefaultUnit = cargoType.DefaultUnit,
                SortOrder = cargoType.SortOrder,
                IsActive = cargoType.IsActive
            };
        }

        public async Task UpdateAsync(int id, UpdateCargoTypeRequest request, CancellationToken ct = default)
        {
            var cargoType = await GetOrThrowAsync(id, ct);
            cargoType.Update(request.Name, request.DefaultUnit, request.SortOrder);
            _repo.Update(cargoType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var cargoType = await GetOrThrowAsync(id, ct);
            cargoType.Activate();
            _repo.Update(cargoType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var cargoType = await GetOrThrowAsync(id, ct);
            cargoType.Deactivate();
            _repo.Update(cargoType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var cargoType = await GetOrThrowAsync(id, ct);

            // Step 4 adds the "used in voyage cargo" check here, once VoyageCargo exists.

            _repo.HardDelete(cargoType);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<CargoType> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Cargo type {id} not found.");

        private static readonly Expression<Func<CargoType, CargoTypeResponse>> ToResponse =
            x => new CargoTypeResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Category = x.Category,
                DefaultUnit = x.DefaultUnit,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            };
    }
}