using Marilog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Responses
{
    public class VoyageResponse
    {
        public int VoyageId { get; set; }
        public string VoyageNumber { get; set; } = null!;
        public int VesselId { get; set; }
        public string VesselName { get; set; } = null!;
        public DateOnly VoyageMonth { get; set; }
        public VoyageStatus Status { get; set; }
        public int? MasterContractId { get; set; }
        public string? MasterFullName { get; set; }
        public int? DeparturePortId { get; set; }
        public string? DeparturePortName { get; set; }
        public int? ArrivalPortId { get; set; }
        public string? ArrivalPortName { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string? CargoType { get; set; }
        public decimal? CargoQuantityMT { get; set; }
        public decimal CashOnBoard { get; set; }
        public decimal CigarettesOnBoard { get; set; }
        public decimal PreviousMasterBalance { get; set; }
        public string? Notes { get; set; }

        public IReadOnlyList<VoyageStopResponse> Stops { get; set; } = new List<VoyageStopResponse>();
    }
}
