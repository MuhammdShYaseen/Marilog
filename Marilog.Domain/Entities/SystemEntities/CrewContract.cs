using Marilog.Domain.Common;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Rules;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class CrewContract : Entity
    {
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
        public int? DurationInMonth { get; private set; }
        public WagesPaidBy WagesPaidBy { get; private set; } = WagesPaidBy.Office;

        private CrewContract() { }
        public static CrewContract Create(int durationInMonth, int personId, int vesselId, int rankId,
            decimal monthlyWage, DateOnly signOnDate, int? signOnPort = null,
            string? notes = null)
        {
            if (durationInMonth <= 0) throw new ArgumentException("Invalid durationInMonth");
            if (personId <= 0) throw new ArgumentException("Invalid PersonID.");
            if (vesselId <= 0) throw new ArgumentException("Invalid VesselID.");
            if (rankId <= 0) throw new ArgumentException("Invalid RankID.");
            if (monthlyWage < 0) throw new ArgumentException("Wage cannot be negative.");

            return new CrewContract
            {
                DurationInMonth = durationInMonth,
                PersonID = personId,
                VesselID = vesselId,
                RankID = rankId,
                MonthlyWage = monthlyWage,
                SignOnDate = signOnDate,
                SignOnPort = signOnPort,
                Notes = notes
            };
        }

        public void Update(int durationInMonth, int personId, int vesselId, int rankId,
                                       decimal monthlyWage, DateOnly signOnDate,
                                       int? signOnPort = null, string? notes = null)
        {
            if (monthlyWage < 0) throw new ArgumentException("Wage cannot be negative.");
            DurationInMonth = durationInMonth;
            PersonID = personId;
            VesselID = vesselId;
            RankID = rankId;
            MonthlyWage = monthlyWage;
            SignOnDate = signOnDate;
            SignOnPort = signOnPort;
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

        public void SetWagesPaidBy(WagesPaidBy wagesPaidBy)
        {
            if (!Enum.IsDefined(wagesPaidBy))
                throw new ArgumentException("Invalid WagesPaidBy.");

            WagesPaidBy = wagesPaidBy;
            Touch();
        }

        // Inclusive days, same rule as payroll (CrewWageRules).
        public int ContractDurationDays() =>
            CrewWageRules.CountServiceDays(SignOnDate, SignOffDate, SignOnDate, ServiceEnd());

        public decimal TotalWageEarned() =>
            ServiceEnd() < SignOnDate
                ? 0m
                : CrewWageRules.PeriodWage(MonthlyWage, SignOnDate, SignOffDate, SignOnDate, ServiceEnd());

        private DateOnly ServiceEnd() =>
            SignOffDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
    }
}