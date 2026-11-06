using Marilog.Domain.Common;
using Marilog.Domain.ValueObjects.Cargo;
using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities.SystemEntities
{
    

    // ── Aggregate Root ──────────────────────────────────────────────────────────
    public class Voyage : Entity
    {
        public int VesselID { get; private set; }
        public Vessel Vessel { get; private set; } = null!;
        public string VoyageNumber { get; private set; } = null!;
        public DateOnly VoyageMonth { get; private set; }
        public int? MasterContractID { get; private set; }
        public CrewContract? MasterContract { get; private set; }
        public int? DeparturePortID { get; private set; }
        public Port? DeparturePort { get; private set; }
        public int? ArrivalPortID { get; private set; }
        public Port? ArrivalPort { get; private set; }
        public DateTime? DepartureDate { get; private set; }
        public DateTime? ArrivalDate { get; private set; }
        public string? CargoType { get; private set; }
        public decimal? CargoQuantityMT { get; private set; }
        public VoyageStatus Status { get; private set; } = VoyageStatus.PLANNED;
        public decimal PreviousMasterBalance { get; private set; }
        public decimal CashOnBoard { get; private set; }
        public decimal CigarettesOnBoard { get; private set; }
        public string? Notes { get; private set; }
        private readonly List<BillOfLading> _billsOfLading = new();

        public IReadOnlyCollection<BillOfLading> BillsOfLading =>
            _billsOfLading.AsReadOnly();

        private readonly List<VoyageStop> _stops = new();
        public IReadOnlyCollection<VoyageStop> Stops => _stops.AsReadOnly();

        private readonly List<VoyageContract> _contracts = new();
        public IReadOnlyCollection<VoyageContract> Contracts => _contracts.AsReadOnly();

        private Voyage() { }
        public static Voyage Create(int vesselId, string voyageNumber, DateOnly voyageMonth,
            int? masterContractId = null, int? departurePortId = null, int? arrivalPortId = null,
            DateTime? departureDate = null, DateTime? arrivalDate = null,
            string? cargoType = null, decimal? cargoQuantityMt = null,
            decimal previousMasterBalance = 0, decimal cashOnBoard = 0,
            decimal cigarettesOnBoard = 0, string? notes = null)
        {
            if (vesselId <= 0) throw new ArgumentException("Invalid VesselID.");
            ArgumentException.ThrowIfNullOrWhiteSpace(voyageNumber);
            if (departureDate.HasValue && arrivalDate.HasValue && arrivalDate < departureDate)
                throw new InvalidOperationException("ArrivalDate cannot be before DepartureDate.");

            return new Voyage
            {
                VesselID = vesselId,
                VoyageNumber = voyageNumber,
                VoyageMonth = voyageMonth,
                MasterContractID = masterContractId,
                DeparturePortID = departurePortId,
                ArrivalPortID = arrivalPortId,
                DepartureDate = departureDate,
                ArrivalDate = arrivalDate,
                CargoType = cargoType,
                CargoQuantityMT = cargoQuantityMt,
                PreviousMasterBalance = previousMasterBalance,
                CashOnBoard = cashOnBoard,
                CigarettesOnBoard = cigarettesOnBoard,
                Notes = notes
            };
        }

        public void Update(int? departurePortId, int? arrivalPortId,
            DateTime? departureDate, DateTime? arrivalDate,
            string? cargoType, decimal? cargoQuantityMt, string? notes)
        {
            EnsureNotCancelled();
            if (departureDate.HasValue && arrivalDate.HasValue && arrivalDate < departureDate)
                throw new InvalidOperationException("ArrivalDate cannot be before DepartureDate.");

            DeparturePortID = departurePortId;
            ArrivalPortID = arrivalPortId;
            DepartureDate = departureDate;
            ArrivalDate = arrivalDate;
            CargoType = cargoType;
            CargoQuantityMT = cargoQuantityMt;
            Notes = notes;
            Touch();
        }

        public void UpdateFinancials(decimal cashOnBoard, decimal cigarettesOnBoard, decimal previousMasterBalance)
        {
            EnsureNotCancelled();
            CashOnBoard = cashOnBoard;
            CigarettesOnBoard = cigarettesOnBoard;
            PreviousMasterBalance = previousMasterBalance;
            Touch();
        }

        public void AssignMaster(int contractId)
        {
            if (contractId <= 0) throw new ArgumentException("Invalid ContractID.");
            MasterContractID = contractId;
            Touch();
        }

        // ── Status transitions ──────────────────────────────────────────────────
        public void Start()
        {
            if (Status != VoyageStatus.PLANNED)
                throw new InvalidOperationException("Only PLANNED voyages can be started.");
            Status = VoyageStatus.UNDERWAY;
            Touch();
        }

        public void Complete()
        {
            if (Status != VoyageStatus.UNDERWAY)
                throw new InvalidOperationException("Only UNDERWAY voyages can be completed.");
            Status = VoyageStatus.COMPLETED;
            Touch();
        }

        public void Cancel()
        {
            if (Status == VoyageStatus.COMPLETED)
                throw new InvalidOperationException("Completed voyages cannot be cancelled.");
            Status = VoyageStatus.CANCELLED;
            Touch();
        }

        // ── Stop management (owned by this aggregate) ───────────────────────────
        public VoyageStop AddStop(int portId, int stopOrder,
            DateTime? arrivalDate = null, DateTime? departureDate = null,
            string? purposeOfCall = null, string? notes = null)
        {
            if (_stops.Any(s => s.StopOrder == stopOrder))
                throw new InvalidOperationException($"StopOrder {stopOrder} already exists.");

            var stop = VoyageStop.Create(Id, portId, stopOrder,
                arrivalDate, departureDate, purposeOfCall, notes);
            _stops.Add(stop);
            Touch();
            return stop;
        }

        public void UpdateStop(int stopOrder, DateTime? arrivalDate,
            DateTime? departureDate, string? purposeOfCall, string? notes)
        {
            var stop = _stops.FirstOrDefault(s => s.StopOrder == stopOrder)
                ?? throw new InvalidOperationException($"Stop {stopOrder} not found.");
            stop.Update(arrivalDate, departureDate, purposeOfCall, notes);
            Touch();
        }

        public void RemoveStop(int stopOrder)
        {
            var stop = _stops.FirstOrDefault(s => s.StopOrder == stopOrder)
                ?? throw new InvalidOperationException($"Stop {stopOrder} not found.");
            _stops.Remove(stop);
            Touch();
        }

        // ── Contracts (many-to-many) ─────────────────────────────────────────────
        public void LinkContract(int contractId)
        {
            EnsureNotCancelled();
            if (contractId <= 0) throw new ArgumentException("Invalid ContractID.");

            if (_contracts.Any(c => c.ContractID == contractId))
                throw new InvalidOperationException("This contract is already linked to the voyage.");

            _contracts.Add(new VoyageContract(contractId));
            Touch();
        }

        public void UnlinkContract(int contractId)
        {
            EnsureNotCancelled();

            var link = _contracts.FirstOrDefault(c => c.ContractID == contractId)
                ?? throw new InvalidOperationException("This contract is not linked to the voyage.");

            if (_stops.Any(s => s.Cargo.Any(c => c.ContractID == contractId)))
                throw new InvalidOperationException(
                    "Cargo lines on this voyage use this contract. Change them first.");

            _contracts.Remove(link);
            Touch();
        }

        // ── Cargo lines (on stops) ───────────────────────────────────────────────
        public VoyageCargo AddCargo(int stopOrder, int cargoTypeId, CargoCategory category,
            CargoOperation operation, decimal quantity, CargoUnit unit,
            decimal? weightMt = null, int? contractId = null, string? notes = null,
            BulkCargoDetail? bulk = null, GeneralCargoDetail? general = null)
        {
            EnsureNotCancelled();
            EnsureContractLinked(contractId);

            var stop = _stops.FirstOrDefault(s => s.StopOrder == stopOrder)
                ?? throw new InvalidOperationException($"Stop {stopOrder} not found.");

            var line = VoyageCargo.Create(cargoTypeId, category, operation, quantity, unit,
                weightMt, contractId, notes, bulk, general);

            stop.AddCargo(line);
            Touch();
            return line;
        }

        public void UpdateCargo(int voyageCargoId, int cargoTypeId, CargoCategory category,
            CargoOperation operation, decimal quantity, CargoUnit unit,
            decimal? weightMt = null, int? contractId = null, string? notes = null,
            BulkCargoDetail? bulk = null, GeneralCargoDetail? general = null)
        {
            EnsureNotCancelled();
            EnsureContractLinked(contractId);

            var line = _stops.SelectMany(s => s.Cargo).FirstOrDefault(c => c.VoyageCargoID == voyageCargoId)
                ?? throw new InvalidOperationException($"Cargo line {voyageCargoId} not found.");

            line.Update(cargoTypeId, category, operation, quantity, unit,
                weightMt, contractId, notes, bulk, general);
            Touch();
        }

        public void RemoveCargo(int voyageCargoId)
        {
            EnsureNotCancelled();

            var stop = _stops.FirstOrDefault(s => s.Cargo.Any(c => c.VoyageCargoID == voyageCargoId))
                ?? throw new InvalidOperationException($"Cargo line {voyageCargoId} not found.");

            stop.RemoveCargo(stop.Cargo.First(c => c.VoyageCargoID == voyageCargoId));
            Touch();
        }

        // A cargo line may only reference a contract that is linked to this voyage.
        private void EnsureContractLinked(int? contractId)
        {
            if (contractId is null) return;

            if (!_contracts.Any(c => c.ContractID == contractId))
                throw new InvalidOperationException(
                    "Link the contract to the voyage before assigning cargo to it.");
        }
        private void EnsureNotCancelled()
        {
            if (Status == VoyageStatus.CANCELLED)
                throw new InvalidOperationException("Cancelled voyages cannot be modified.");
        }
    }
}
