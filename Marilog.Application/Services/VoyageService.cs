using Marilog.Application.DTOs;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
{
    public class VoyageService : IVoyageService
    {
        private readonly IRepository<Voyage> _repo;
        private readonly IRepository<Vessel> _vesselRepo;
        private readonly IRepository<CrewContract> _contractRepo;

        public VoyageService(
            IRepository<Voyage> repo,
            IRepository<Vessel> vesselRepo,
            IRepository<CrewContract> contractRepo)
        {
            _repo = repo;
            _vesselRepo = vesselRepo;
            _contractRepo = contractRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<VoyageResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<VoyageResponse?> GetWithStopsAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithStops)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetByVesselAsync(int vesselId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.VesselID == vesselId)
                .OrderByDescending(x => x.VoyageMonth)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetByMonthAsync(DateOnly month,
            CancellationToken ct = default)
        {
            var firstDay = new DateOnly(month.Year, month.Month, 1);

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.VoyageMonth == firstDay)
                .OrderBy(x => x.VoyageNumber)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetByStatusAsync(VoyageStatus status,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.Status == status)
                          .OrderByDescending(x => x.DepartureDate)
                          .Select (ToResponse)
                          .ToListAsync(ct);

        public async Task<VoyageResponse?> GetCurrentVoyageAsync(int vesselId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .AsNoTracking()
                          .Where(x => x.VesselID == vesselId &&
                                      x.Status == VoyageStatus.UNDERWAY)
                          .Select(ToResponseWithStops)
                          .FirstOrDefaultAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<VoyageResponse> CreateAsync(int vesselId, string voyageNumber,
            DateOnly voyageMonth, int? masterContractId = null,
            int? departurePortId = null, int? arrivalPortId = null,
            DateTime? departureDate = null, DateTime? arrivalDate = null,
            string? cargoType = null, decimal? cargoQuantityMt = null,
            string? notes = null, CancellationToken ct = default)
        {
            await EnsureVesselActiveAsync(vesselId, ct);
            await EnsureUniqueVoyageNumberAsync(voyageNumber, excludeId: null, ct);

            if (masterContractId.HasValue)
                await EnsureContractActiveAsync(masterContractId.Value, ct);

            var voyage = Voyage.Create(vesselId, voyageNumber, voyageMonth,
                                       masterContractId, departurePortId, arrivalPortId,
                                       departureDate, arrivalDate, cargoType,
                                       cargoQuantityMt, notes: notes);
            await _repo.AddAsync(voyage, ct);
            await _repo.SaveChangesAsync(ct);
            return new VoyageResponse
            {
                VesselId = vesselId,
                VoyageNumber = voyageNumber,
                VoyageMonth = voyageMonth.Month,
                DepartureDate = departureDate,
                DeparturePortId = departurePortId,
                MasterContractId = masterContractId,
                ArrivalDate = arrivalDate,
                ArrivalPortId = arrivalPortId,
                CargoQuantityMT = cargoQuantityMt,
                Notes = notes
            };
        }

        public async Task UpdateAsync(int id, int? departurePortId, int? arrivalPortId,
            DateTime? departureDate, DateTime? arrivalDate,
            string? cargoType, decimal? cargoQuantityMt,
            string? notes, CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            voyage.Update(departurePortId, arrivalPortId, departureDate, arrivalDate,
                          cargoType, cargoQuantityMt, notes);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateFinancialsAsync(int id, decimal cashOnBoard,
            decimal cigarettesOnBoard, decimal previousMasterBalance,
            CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            voyage.UpdateFinancials(cashOnBoard, cigarettesOnBoard, previousMasterBalance);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task AssignMasterAsync(int id, int contractId,
            CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            await EnsureContractActiveAsync(contractId, ct);
            voyage.AssignMaster(contractId);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task StartAsync(int id, CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            voyage.Start();
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task CompleteAsync(int id, CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            voyage.Complete();
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task CancelAsync(int id, CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            voyage.Cancel();
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            if (voyage.Status == VoyageStatus.UNDERWAY)
                throw new InvalidOperationException(
                    "Cannot delete a voyage that is currently underway.");
            _repo.HardDelete(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Stops ─────────────────────────────────────────────────────────────────

        public async Task<VoyageStop> AddStopAsync(int voyageId, int portId, int stopOrder,
            DateTime? arrivalDate = null, DateTime? departureDate = null,
            string? purposeOfCall = null, string? notes = null,
            CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            var stop = voyage.AddStop(portId, stopOrder, arrivalDate, departureDate,
                                        purposeOfCall, notes);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
            return stop;
        }

        public async Task UpdateStopAsync(int voyageId, int stopOrder,
            DateTime? arrivalDate, DateTime? departureDate,
            string? purposeOfCall, string? notes,
            CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            voyage.UpdateStop(stopOrder, arrivalDate, departureDate, purposeOfCall, notes);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveStopAsync(int voyageId, int stopOrder,
            CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            voyage.RemoveStop(stopOrder);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Voyage> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Voyage {id} not found.");

        private async Task<Voyage> GetWithStopsOrThrowAsync(int id, CancellationToken ct)
            => await _repo.Query()
                          .Include(x => x.Stops)
                          .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"Voyage {id} not found.");

        private async Task EnsureVesselActiveAsync(int vesselId, CancellationToken ct)
        {
            var exists = await _vesselRepo.Query()
                .AnyAsync(x => x.Id == vesselId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Vessel {vesselId} not found or inactive.");
        }

        private async Task EnsureContractActiveAsync(int contractId, CancellationToken ct)
        {
            var exists = await _contractRepo.Query()
                .AnyAsync(x => x.Id == contractId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException(
                    $"CrewContract {contractId} not found or inactive.");
        }

        private async Task EnsureUniqueVoyageNumberAsync(string voyageNumber,
            int? excludeId, CancellationToken ct)
        {
            var conflict = await _repo.Query()
                .AnyAsync(x => x.VoyageNumber == voyageNumber &&
                               (excludeId == null || x.Id != excludeId), ct);
            if (conflict)
                throw new InvalidOperationException(
                    $"Voyage number '{voyageNumber}' already exists.");
        }

        private static readonly Expression<Func<Voyage, VoyageResponse>> ToResponse =
        x => new VoyageResponse
        {
            VoyageId = x.Id,
            VoyageNumber = x.VoyageNumber,

            VesselId = x.VesselID,
            VesselName = x.Vessel.VesselName,

            VoyageMonth = x.VoyageMonth.Month, // أو حسب تصميمك

            Status = x.Status,

            MasterContractId = x.MasterContractID,
            MasterFullName = x.MasterContract != null && x.MasterContract.Person != null
                ? x.MasterContract.Person.FullName
                : null,

            DeparturePortId = x.DeparturePortID,
            DeparturePortName = x.DeparturePort != null
                ? x.DeparturePort.PortName
                : null,

            ArrivalPortId = x.ArrivalPortID,
            ArrivalPortName = x.ArrivalPort != null
                ? x.ArrivalPort.PortName
                : null,

            DepartureDate = x.DepartureDate,
            ArrivalDate = x.ArrivalDate,

            CargoType = x.CargoType,
            CargoQuantityMT = x.CargoQuantityMT,

            CashOnBoard = x.CashOnBoard,
            CigarettesOnBoard = x.CigarettesOnBoard,
            PreviousMasterBalance = x.PreviousMasterBalance,

            Notes = x.Notes,

            // ⚠️ مهم
            Stops = new List<VoyageStopResponse>() // يتم تعبئتها في Projection آخر
        };


        private static readonly Expression<Func<Voyage, VoyageResponse>> ToResponseWithStops =
        x => new VoyageResponse
        {
            VoyageId = x.Id,
            VoyageNumber = x.VoyageNumber,

            VesselId = x.VesselID,
            VesselName = x.Vessel.VesselName,

            VoyageMonth = x.VoyageMonth.Month, // أو حسب تصميمك

            Status = x.Status,

            MasterContractId = x.MasterContractID,
            MasterFullName = x.MasterContract != null && x.MasterContract.Person != null
                ? x.MasterContract.Person.FullName
                : null,

            DeparturePortId = x.DeparturePortID,
            DeparturePortName = x.DeparturePort != null
                ? x.DeparturePort.PortName
                : null,

            ArrivalPortId = x.ArrivalPortID,
            ArrivalPortName = x.ArrivalPort != null
                ? x.ArrivalPort.PortName
                : null,

            DepartureDate = x.DepartureDate,
            ArrivalDate = x.ArrivalDate,

            CargoType = x.CargoType,
            CargoQuantityMT = x.CargoQuantityMT,

            CashOnBoard = x.CashOnBoard,
            CigarettesOnBoard = x.CigarettesOnBoard,
            PreviousMasterBalance = x.PreviousMasterBalance,

            Notes = x.Notes,

            // ⚠️ مهم
            Stops = x.Stops
            .OrderBy(s => s.StopOrder)
            .Select(s => new VoyageStopResponse 
            {
                ArrivalDate = s.ArrivalDate,
                StopOrder = s.StopOrder,
                DepartureDate = s.DepartureDate,
                Notes = s.Notes,
                PortId = s.PortID,
                PortName = s.Port.PortName,
                PurposeOfCall = s.PurposeOfCall
            }).ToList(),
        };
    }
}
