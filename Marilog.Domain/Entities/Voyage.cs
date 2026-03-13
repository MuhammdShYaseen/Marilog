using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities
{
    public enum VoyageStatus { PLANNED, UNDERWAY, COMPLETED, CANCELLED }

    // ── Aggregate Root ──────────────────────────────────────────────────────────
    public class Voyage : Entity
    {
        public int VoyageID { get; private set; }
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

        private readonly List<VoyageStop> _stops = new();
        public IReadOnlyCollection<VoyageStop> Stops => _stops.AsReadOnly();

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

        public void UpdateFinancials(decimal cashOnBoard, decimal cigarettesOnBoard,
            decimal previousMasterBalance)
        {
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

            var stop = VoyageStop.Create(VoyageID, portId, stopOrder,
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
    }
}
