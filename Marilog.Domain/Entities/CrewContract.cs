using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    public class CrewContract : Entity
    {
        public int ContractID { get; private set; }
        public int PersonID { get; private set; }
        public Person Person { get; private set; } = null!;
        public int VesselID { get; private set; }
        public Vessel Vessel { get; private set; } = null!;
        public int RankID { get; private set; }
        public Rank Rank { get; private set; } = null!;
        public decimal MonthlyWage { get; private set; }
        public DateOnly SignOnDate { get; private set; }
        public DateOnly? SignOffDate { get; private set; }
        public int? SignOnPort { get; private set; }
        public Port? SignOnPortNav { get; private set; }
        public int? SignOffPort { get; private set; }
        public Port? SignOffPortNav { get; private set; }
        public string? Notes { get; private set; }

        private CrewContract() { }
        public static CrewContract Create(int personId, int vesselId, int rankId,
            decimal monthlyWage, DateOnly signOnDate, int? signOnPort = null,
            string? notes = null)
        {
            if (personId <= 0) throw new ArgumentException("Invalid PersonID.");
            if (vesselId <= 0) throw new ArgumentException("Invalid VesselID.");
            if (rankId <= 0) throw new ArgumentException("Invalid RankID.");
            if (monthlyWage < 0) throw new ArgumentException("Wage cannot be negative.");

            return new CrewContract
            {
                PersonID = personId,
                VesselID = vesselId,
                RankID = rankId,
                MonthlyWage = monthlyWage,
                SignOnDate = signOnDate,
                SignOnPort = signOnPort,
                Notes = notes
            };
        }

        public void Update(decimal monthlyWage, string? notes = null)
        {
            if (monthlyWage < 0) throw new ArgumentException("Wage cannot be negative.");
            MonthlyWage = monthlyWage;
            Notes = notes;
            Touch();
        }

        public void SignOff(DateOnly signOffDate, int? signOffPort = null)
        {
            if (signOffDate < SignOnDate)
                throw new InvalidOperationException("SignOff date cannot be before SignOn date.");

            SignOffDate = signOffDate;
            SignOffPort = signOffPort;
            IsActive = false;
            Touch();
        }

        public int ContractDurationDays() =>
            (SignOffDate.HasValue ? SignOffDate.Value : DateOnly.FromDateTime(DateTime.UtcNow))
                .DayNumber - SignOnDate.DayNumber;

        public decimal TotalWageEarned() =>
            Math.Round(MonthlyWage / 30m * ContractDurationDays(), 2);
    }
}