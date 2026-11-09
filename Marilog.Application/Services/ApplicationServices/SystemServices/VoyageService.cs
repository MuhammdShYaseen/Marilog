using Marilog.Contracts.DTOs.Reports.VoyageReports;
using Marilog.Contracts.DTOs.Requests.VoyageDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.ValueObjects.Cargo;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class VoyageService : IVoyageService
    {
        private readonly IRepository<Voyage> _repo;
        private readonly IRepository<Vessel> _vesselRepo;
        private readonly IRepository<CrewContract> _contractRepo;
        private readonly IRepository<AContract> _aContractRepo;
        private readonly IRepository<CargoType> _cargoTypeRepo;

        public VoyageService(
            IRepository<Voyage> repo,
            IRepository<Vessel> vesselRepo,
            IRepository<CrewContract> contractRepo,
            IRepository<AContract> aContractRepo,
            IRepository<CargoType> cargoTypeRepo)
        {
            _repo = repo;
            _vesselRepo = vesselRepo;
            _contractRepo = contractRepo;
            _aContractRepo = aContractRepo;
            _cargoTypeRepo = cargoTypeRepo;
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
            var result = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithStops)
                .FirstOrDefaultAsync(ct);

            if (result is not null)
                await FillContractNumbersAsync(result, ct);

            return result;
        }
        // ==== الميثود ====
        public async Task<IReadOnlyList<VoyageLookupResponse>> GetAllLookupAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .Where(v => v.IsActive == true)
                .AsNoTracking()
                .Select(ToLookupResponse)
                .ToListAsync(ct);
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

        public async Task<IReadOnlyList<VoyageResponse>> GetActiveVoyagesAsync(CancellationToken ct = default)
        {
            var activeVoyages =  await _repo.Query()
                .AsNoTracking()
                .Where(v => v.IsActive == true)
                .Select (ToResponseWithStops)
                .ToListAsync (ct);
            return activeVoyages;
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetByStatusAsync(VoyageStatus status,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.Status == status)
                          .OrderByDescending(x => x.VoyageMonth)
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

        // ─── Reports ───────────────────────────────────────────────────────────
        public async Task<VoyageReport> GetVoyagesReportAsync(VoyageReportFilterOptions options, CancellationToken ct = default)
        {
            var query = _repo.Query().AsNoTracking();

            if (options.Id.HasValue)
                query = query.Where(x => x.Id == options.Id.Value);

            if (options.VesselId.HasValue)
                query = query.Where(x => x.VesselID == options.VesselId.Value);

            if (options.Month.HasValue)
            {
                var firstDay = new DateOnly(options.Month.Value.Year, options.Month.Value.Month, 1);
                query = query.Where(x => x.VoyageMonth == firstDay);
            }

            if (options.Status.HasValue)
                query = query.Where(x => x.Status == options.Status.Value);

            // Date range filters on VoyageMonth (the month the voyage is booked under)
            if (options.FromDate.HasValue)
            {
                var fromMonth = new DateOnly(options.FromDate.Value.Year, options.FromDate.Value.Month, 1);
                query = query.Where(x => x.VoyageMonth >= fromMonth);
            }

            if (options.ToDate.HasValue)
            {
                var toMonth = new DateOnly(options.ToDate.Value.Year, options.ToDate.Value.Month, 1);
                query = query.Where(x => x.VoyageMonth <= toMonth);
            }

            if (options.OnlyCurrent)
                query = query.Where(x => x.Status == VoyageStatus.UNDERWAY);

            query = query.OrderByDescending(x => x.VoyageMonth)
                         .ThenBy(x => x.VoyageNumber);

            var dbSummary = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    Underway = g.Count(x => x.Status == VoyageStatus.UNDERWAY),
                    Completed = g.Count(x => x.Status == VoyageStatus.COMPLETED),
                    Planned = g.Count(x => x.Status == VoyageStatus.PLANNED),
                })
                .FirstOrDefaultAsync(ct);

            var selector = options.IncludeStops ? ToResponseWithStops : ToResponse;

            var voyages = await query
                .Select(selector)
                .ToListAsync(ct);

            // ─── الرحلات الحالية (UNDERWAY) لكل سفينة ────────────────────────────
            // نستخرجها من النتيجة المجلوبة إن كانت الفلترة لا تمنعها،
            // وإلا نجلبها بـ query مستقل لضمان الاكتمال
            var currentVoyages = options.OnlyCurrent
                  || options.Status == VoyageStatus.UNDERWAY
                    ? voyages.Where(v => v.Status == VoyageStatus.UNDERWAY).ToList()
                    : await _repo.Query()
                 .AsNoTracking()
                 .Where(x => x.Status == VoyageStatus.UNDERWAY)
                 .Select(ToResponseWithStops)
                 .ToListAsync(ct);

            // ─── التجميع حسب الحالة ──────────────────────────────────────────────
            var statusSummary = voyages
                .GroupBy(v => v.Status)
                .Select(g => new StatusSummary
                {
                    Status = g.Key,
                    Count = g.Count(),
                })
                .OrderBy(s => s.Status)
                .ToList();

            // ─── التجميع حسب السفينة ─────────────────────────────────────────────
            var vesselSummary = voyages
                .GroupBy(v => v.VesselId)
                .Select(g => new VesselVoyageSummary
                {
                    VesselId = g.Key,
                    VesselName = g.First().VesselName ?? string.Empty,
                    TotalVoyages = g.Count(),
                    Underway = g.Count(v => v.Status == VoyageStatus.UNDERWAY),
                    Completed = g.Count(v => v.Status == VoyageStatus.COMPLETED),
                    Planned = g.Count(v => v.Status == VoyageStatus.PLANNED),
                })
                .OrderBy(v => v.VesselName)
                .ToList();

            // ─── التجميع الشهري ───────────────────────────────────────────────────
            var monthlySummary = voyages
                .GroupBy(v => new { v.VoyageMonth.Year, v.VoyageMonth.Month })
                .Select(g => new MonthlyVoyageSummary
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalVoyages = g.Count(),
                    Completed = g.Count(v => v.Status == VoyageStatus.COMPLETED),
                    Underway = g.Count(v => v.Status == VoyageStatus.UNDERWAY),
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            // ─── النتيجة النهائية ─────────────────────────────────────────────────
            return new VoyageReport
            {
                Voyages = voyages,
                TotalCount = dbSummary?.TotalCount ?? voyages.Count,
                StatusSummary = statusSummary,
                VesselSummary = vesselSummary,
                MonthlySummary = monthlySummary,
                CurrentVoyages = currentVoyages,
            };
        }
        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<VoyageResponse> CreateAsync(CreateVoyageRequest request, CancellationToken ct = default)
        {
            await EnsureVesselActiveAsync(request.VesselId, ct);
            await EnsureUniqueVoyageNumberAsync(request.VoyageNumber, excludeId: null, ct);

            if (request.MasterContractId.HasValue)
                await EnsureContractActiveAsync(request.MasterContractId.Value, ct);

            var voyage = Voyage.Create(request.VesselId, request.VoyageNumber, request.VoyageMonth,
                masterContractId: request.MasterContractId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                notes: request.Notes);

            await _repo.AddAsync(voyage, ct);
            await _repo.SaveChangesAsync(ct);

            return new VoyageResponse
            {
                VoyageId = voyage.Id,
                VesselId = voyage.VesselID,
                VoyageNumber = voyage.VoyageNumber,
                VoyageMonth = voyage.VoyageMonth,
                MasterContractId = voyage.MasterContractID,
                StartDate = voyage.StartDate,
                EndDate = voyage.EndDate,
                Notes = voyage.Notes
            };
        }

        public async Task<IReadOnlyList<VoyageResponse>> CreateRangeAsync(
    IEnumerable<CreateVoyageRequest> commands,
    CancellationToken ct = default)
        {
            var commandList = commands.ToList();
            if (!commandList.Any())
                return Array.Empty<VoyageResponse>();

            // --- تحقق من التكرار في VoyageNumber داخل الـ batch ---
            var voyageNumberSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in commandList)
            {
                if (!voyageNumberSet.Add(c.VoyageNumber))
                    throw new InvalidOperationException(
                        $"Duplicate VoyageNumber found in the request: '{c.VoyageNumber}'");
            }

            // --- تحقق من الـ VesselIds بـ query واحدة ---
            var vesselIds = commandList.Select(c => c.VesselId).Distinct().ToList();
            var activeVesselIds = await _vesselRepo.Query()
                .Where(v => vesselIds.Contains(v.Id) && v.IsActive)
                .Select(v => v.Id)
                .ToListAsync(ct);

            var inactiveVesselIds = vesselIds.Except(activeVesselIds).ToList();
            if (inactiveVesselIds.Any())
                throw new InvalidOperationException(
                    $"Vessel Id(s) not found or inactive: {string.Join(", ", inactiveVesselIds)}");

            // --- تحقق من التكرار في VoyageNumber في DB بـ query واحدة ---
            var voyageNumbers = voyageNumberSet.ToList();
            var existingNumbers = await _repo.Query()
                .Where(v => voyageNumbers.Contains(v.VoyageNumber))
                .Select(v => v.VoyageNumber)
                .ToListAsync(ct);

            if (existingNumbers.Any())
                throw new InvalidOperationException(
                    $"VoyageNumber(s) already exist: {string.Join(", ", existingNumbers)}");

            // --- تحقق من الـ MasterContractIds بـ query واحدة ---
            var contractIds = commandList
                .Where(c => c.MasterContractId.HasValue)
                .Select(c => c.MasterContractId!.Value)
                .Distinct()
                .ToList();

            if (contractIds.Any())
            {
                var activeContractIds = await _contractRepo.Query()
                    .Where(c => contractIds.Contains(c.Id) && c.IsActive)
                    .Select(c => c.Id)
                    .ToListAsync(ct);

                var inactiveContractIds = contractIds.Except(activeContractIds).ToList();
                if (inactiveContractIds.Any())
                    throw new InvalidOperationException(
                        $"MasterContract Id(s) not found or inactive: {string.Join(", ", inactiveContractIds)}");
            }
            // --- إنشاء دفعة واحدة ---
            var voyages = commandList
                .Select(c => Voyage.Create(c.VesselId, c.VoyageNumber, c.VoyageMonth,
                    masterContractId: c.MasterContractId,
                    startDate: c.StartDate,
                    endDate: c.EndDate,
                    notes: c.Notes))
                .ToList();

            await _repo.AddRangeAsync(voyages, ct);
            await _repo.SaveChangesAsync(ct);

            return voyages
                .Select(voyage => new VoyageResponse
                {
                    VoyageId = voyage.Id,
                    VesselId = voyage.VesselID,
                    VoyageNumber = voyage.VoyageNumber,
                    VoyageMonth = voyage.VoyageMonth,
                    MasterContractId = voyage.MasterContractID,
                    StartDate = voyage.StartDate,
                    EndDate = voyage.EndDate,
                    Notes = voyage.Notes
                })
                .ToList();
        }

        public async Task UpdateAsync(int id, UpdateVoyageRequest request, CancellationToken ct = default)
        {
            var voyage = await GetOrThrowAsync(id, ct);
            voyage.Update(request.StartDate, request.EndDate, request.Notes);
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
            if (voyage.Status is VoyageStatus.UNDERWAY or VoyageStatus.COMPLETED)
                throw new InvalidOperationException(
                     "Underway or completed voyages cannot be deleted.");
            _repo.HardDelete(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Stops ─────────────────────────────────────────────────────────────────

        public async Task<VoyageStopResponse> AddStopAsync(int voyageId, AddVoyageStopRequest request,
    CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);

            // A stop whose port was soft-deleted is hidden by the query filter
            // but still occupies its StopOrder in the unique index.
            var orderTaken = await _repo.Query()
                .IgnoreQueryFilters()
                .Where(v => v.Id == voyageId)
                .SelectMany(v => v.Stops)
                .AnyAsync(s => s.StopOrder == request.StopOrder, ct);
            if (orderTaken)
                throw new InvalidOperationException(
                    $"StopOrder {request.StopOrder} is already used on this voyage.");

            voyage.AddStop(request.PortId, request.StopOrder, request.ArrivalDate,
                request.DepartureDate, request.PurposeOfCall, request.Notes);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);

            return await _repo.Query()
                .AsNoTracking()
                .Where(v => v.Id == voyageId)
                .SelectMany(v => v.Stops)
                .Where(s => s.StopOrder == request.StopOrder)
                .Select(s => new VoyageStopResponse
                {
                    StopOrder = s.StopOrder,
                    StopId = s.StopID,
                    PortId = s.PortID,
                    PortName = s.Port.PortName,
                    ArrivalDate = s.ArrivalDate,
                    DepartureDate = s.DepartureDate,
                    PurposeOfCall = s.PurposeOfCall,
                    Notes = s.Notes
                })
                .FirstAsync(ct);
        }

        public async Task UpdateStopAsync(int voyageId, int stopOrder, UpdateVoyageStopRequest request, CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            voyage.UpdateStop(stopOrder, request.ArrivalDate, request.DepartureDate,
                request.PurposeOfCall, request.Notes);
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



        // ── Contracts ─────────────────────────────────────────────────────────────

        public async Task LinkContractAsync(int voyageId, int contractId, CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);

            if (!await _aContractRepo.Query().AnyAsync(c => c.Id == contractId, ct))
                throw new KeyNotFoundException($"Contract {contractId} not found.");

            voyage.LinkContract(contractId);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UnlinkContractAsync(int voyageId, int contractId, CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            voyage.UnlinkContract(contractId);
            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Cargo lines ───────────────────────────────────────────────────────────

        public async Task AddCargoAsync(int voyageId, int stopOrder, SaveVoyageCargoRequest request,
            CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            var category = await GetCargoCategoryAsync(request.CargoTypeId, requireActive: true, ct);

            voyage.AddCargo(stopOrder, request.CargoTypeId, category,
                request.Operation, request.Quantity, request.Unit,
                request.WeightMt, request.ContractId, request.Notes,
                BulkCargoDetail.Create(request.DraftSurveyMt, request.ShoreFigureMt),
                GeneralCargoDetail.Create(request.PackageCount, request.PackageType, request.VolumeCbm));

            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateCargoAsync(int voyageId, int voyageCargoId, SaveVoyageCargoRequest request,
            CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            var category = await GetCargoCategoryAsync(request.CargoTypeId, requireActive: false, ct);

            voyage.UpdateCargo(voyageCargoId, request.CargoTypeId, category,
                request.Operation, request.Quantity, request.Unit,
                request.WeightMt, request.ContractId, request.Notes,
                BulkCargoDetail.Create(request.DraftSurveyMt, request.ShoreFigureMt),
                GeneralCargoDetail.Create(request.PackageCount, request.PackageType, request.VolumeCbm));

            _repo.Update(voyage);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveCargoAsync(int voyageId, int voyageCargoId, CancellationToken ct = default)
        {
            var voyage = await GetWithStopsOrThrowAsync(voyageId, ct);
            voyage.RemoveCargo(voyageCargoId);
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
                      .ThenInclude(s => s.Cargo)
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

        private async Task<CargoCategory> GetCargoCategoryAsync(int cargoTypeId, bool requireActive,
    CancellationToken ct)
        {
            var type = await _cargoTypeRepo.Query()
                .AsNoTracking()
                .Where(x => x.Id == cargoTypeId)
                .Select(x => new { x.Category, x.IsActive })
                .FirstOrDefaultAsync(ct)
                ?? throw new KeyNotFoundException($"Cargo type {cargoTypeId} not found.");

            if (requireActive && !type.IsActive)
                throw new InvalidOperationException("This cargo type is inactive.");

            return type.Category;
        }

        // VoyageContract has no navigation to AContract — numbers are filled in one extra query.
        private async Task FillContractNumbersAsync(VoyageResponse voyage, CancellationToken ct)
        {
            var ids = voyage.Contracts.Select(c => c.ContractId).ToList();
            if (ids.Count == 0) return;

            var numbers = await _aContractRepo.Query()
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .Select(c => new { c.Id, c.ContractNumber })
                .ToDictionaryAsync(c => c.Id, c => c.ContractNumber, ct);

            foreach (var c in voyage.Contracts)
                c.ContractNumber = numbers.GetValueOrDefault(c.ContractId) ?? $"#{c.ContractId}";

            foreach (var line in voyage.Stops.SelectMany(s => s.Cargo).Where(l => l.ContractId.HasValue))
                line.ContractNumber = numbers.GetValueOrDefault(line.ContractId!.Value) ?? $"#{line.ContractId}";
        }

        private static readonly Expression<Func<Voyage, VoyageResponse>> ToResponse =
        x => new VoyageResponse
        {
            VoyageId = x.Id,
            VoyageNumber = x.VoyageNumber,

            VesselId = x.VesselID,
            VesselName = x.Vessel.VesselName,

            VoyageMonth = x.VoyageMonth, // أو حسب تصميمك

            Status = x.Status,

            MasterContractId = x.MasterContractID,
            MasterFullName = x.MasterContract != null && x.MasterContract.Person != null
                ? x.MasterContract.Person.FullName
                : null,

            // Route summary — derived from stops (first stop = departure, last stop = arrival)
            DeparturePortId = x.Stops.OrderBy(s => s.StopOrder).Select(s => (int?)s.PortID).FirstOrDefault(),
            DeparturePortName = x.Stops.OrderBy(s => s.StopOrder).Select(s => s.Port.PortName).FirstOrDefault(),
            DepartureDate = x.Stops.OrderBy(s => s.StopOrder).Select(s => s.DepartureDate).FirstOrDefault(),

            ArrivalPortId = x.Stops.OrderByDescending(s => s.StopOrder).Select(s => (int?)s.PortID).FirstOrDefault(),
            ArrivalPortName = x.Stops.OrderByDescending(s => s.StopOrder).Select(s => s.Port.PortName).FirstOrDefault(),
            ArrivalDate = x.Stops.OrderByDescending(s => s.StopOrder).Select(s => s.ArrivalDate).FirstOrDefault(),

            // Cargo summary — derived from loaded cargo lines
            CargoType = x.Stops.SelectMany(s => s.Cargo)
                .Where(c => c.Operation == CargoOperation.Load)
                .OrderBy(c => c.VoyageCargoID)
                .Select(c => c.CargoType.Name)
                .FirstOrDefault(),
            // null (not 0) when nothing is loaded — EF wraps SUM in COALESCE(…, 0)
            CargoQuantityMT = x.Stops.SelectMany(s => s.Cargo).Any(c => c.Operation == CargoOperation.Load)
                ? x.Stops.SelectMany(s => s.Cargo)
                    .Where(c => c.Operation == CargoOperation.Load)
                    .Sum(c => c.WeightMt)
                : null,

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

            VoyageMonth = x.VoyageMonth, // أو حسب تصميمك

            Status = x.Status,

            MasterContractId = x.MasterContractID,
            MasterFullName = x.MasterContract != null && x.MasterContract.Person != null
                ? x.MasterContract.Person.FullName
                : null,

            // Route summary — derived from stops (first stop = departure, last stop = arrival)
            DeparturePortId = x.Stops.OrderBy(s => s.StopOrder).Select(s => (int?)s.PortID).FirstOrDefault(),
            DeparturePortName = x.Stops.OrderBy(s => s.StopOrder).Select(s => s.Port.PortName).FirstOrDefault(),
            DepartureDate = x.Stops.OrderBy(s => s.StopOrder).Select(s => s.DepartureDate).FirstOrDefault(),

            ArrivalPortId = x.Stops.OrderByDescending(s => s.StopOrder).Select(s => (int?)s.PortID).FirstOrDefault(),
            ArrivalPortName = x.Stops.OrderByDescending(s => s.StopOrder).Select(s => s.Port.PortName).FirstOrDefault(),
            ArrivalDate = x.Stops.OrderByDescending(s => s.StopOrder).Select(s => s.ArrivalDate).FirstOrDefault(),

            StartDate = x.StartDate,
            EndDate = x.EndDate,
            // Cargo summary — derived from loaded cargo lines
            CargoType = x.Stops.SelectMany(s => s.Cargo)
                .Where(c => c.Operation == CargoOperation.Load)
                .OrderBy(c => c.VoyageCargoID)
                .Select(c => c.CargoType.Name)
                .FirstOrDefault(),
            // null (not 0) when nothing is loaded — EF wraps SUM in COALESCE(…, 0)
            CargoQuantityMT = x.Stops.SelectMany(s => s.Cargo).Any(c => c.Operation == CargoOperation.Load)
                ? x.Stops.SelectMany(s => s.Cargo)
                    .Where(c => c.Operation == CargoOperation.Load)
                    .Sum(c => c.WeightMt)
                : null,

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
                StopId = s.StopID,
                PurposeOfCall = s.PurposeOfCall,
                Cargo = s.Cargo
                    .OrderBy(c => c.VoyageCargoID)
                    .Select(c => new VoyageCargoResponse
                    {
                        VoyageCargoId = c.VoyageCargoID,
                        CargoTypeId = c.CargoTypeID,
                        CargoTypeName = c.CargoType.Name,
                        Category = c.CargoType.Category,
                        Operation = c.Operation,
                        Quantity = c.Quantity,
                        Unit = c.Unit,
                        WeightMt = c.WeightMt,
                        ContractId = c.ContractID,
                        Notes = c.Notes,
                        DraftSurveyMt = c.Bulk!.DraftSurveyMt,
                        ShoreFigureMt = c.Bulk!.ShoreFigureMt,
                        PackageCount = c.General!.PackageCount,
                        PackageType = c.General!.PackageType,
                        VolumeCbm = c.General!.VolumeCbm
                    }).ToList()
            }).ToList(),

            Contracts = x.Contracts
                .Select(c => new VoyageContractResponse { ContractId = c.ContractID })
                .ToList()
        };

        private static readonly Expression<Func<Voyage, VoyageLookupResponse>> ToLookupResponse = x => new VoyageLookupResponse
        {
            Id = x.Id,
            VesselId = x.VesselID,
            DisplayText = x.Vessel.VesselName + " | " + x.VoyageNumber
            + " - " + x.VoyageMonth.Year
            + " (" + (x.Stops.OrderBy(s => s.StopOrder).Select(s => s.Port.PortName).FirstOrDefault() ?? "?")
            + " → " + (x.Stops.OrderByDescending(s => s.StopOrder).Select(s => s.Port.PortName).FirstOrDefault() ?? "?") + ")"
        };
    }
}
