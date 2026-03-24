

using Marilog.Domain.Entities;

namespace Marilog.Application.DTOs
{
    public class CrewContractResponse
    {
        public int ContractId { get; set; }
        public int PersonId { get; set; }
        public string PersonFullName { get; set; } = null!;
        public int VesselId { get; set; }
        public string VesselName { get; set; } = null!;
        public int RankId { get; set; }
        public string RankName { get; set; } = null!;
        public Department RankDepartment { get; set; }
        public decimal MonthlyWage { get; set; }
        public DateOnly SignOnDate { get; set; }
        public DateOnly? SignOffDate { get; set; }
        public int? SignOnPort { get; set; }
        public string? SignOnPortName { get; set; }
        public int? SignOffPort { get; set; }
        public string? SignOffPortName { get; set; }
        public bool IsActive { get; set; }

        // Domain-calculated fields → احسبها في الخدمة وليس هنا
        public int? ContractDurationDays { get; set; }
        public decimal? TotalWageEarned { get; set; }
        public PersonResponse Person { get; private set; } = new PersonResponse();
        public VesselResponse Vessel { get; private set; } = new VesselResponse();
        public RankResponse Rank { get; private set; } = new RankResponse();
        public PortResponse? SignOnPortNav { get; private set; } = new PortResponse();
        public PortResponse? SignOffPortNav { get; private set; } = new PortResponse();

    }
}
