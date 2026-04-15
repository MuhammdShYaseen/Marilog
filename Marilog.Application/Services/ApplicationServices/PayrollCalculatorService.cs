using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Application.Services.ApplicationServices
{
    public class PayrollCalculatorService : IPayrollCalculatorService
    {
        /// <summary>
        /// Prorates MonthlyWage based on actual days worked in the month.
        /// Formula: (MonthlyWage / totalDaysInMonth) * workingDays
        /// Rounded to 2 decimal places.
        /// </summary>
        public decimal CalculateBasicWage(decimal monthlyWage, int workingDays,
            int totalDaysInMonth)
        {
            if (monthlyWage    <  0) throw new ArgumentException("MonthlyWage cannot be negative.");
            if (workingDays    <= 0) throw new ArgumentException("WorkingDays must be positive.");
            if (totalDaysInMonth <= 0) throw new ArgumentException("TotalDaysInMonth must be positive.");
            if (workingDays > totalDaysInMonth)
                throw new ArgumentException("WorkingDays cannot exceed TotalDaysInMonth.");

            return Math.Round(monthlyWage / totalDaysInMonth * workingDays, 2,
                              MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Returns the number of days the crew member served within the given month,
        /// clamped to the month boundaries.
        ///
        /// Examples:
        ///   SignOn: 2025-03-10, SignOff: null,       Month: March  → 22 days (10→31)
        ///   SignOn: 2025-03-10, SignOff: 2025-03-25, Month: March  → 16 days (10→25)
        ///   SignOn: 2025-02-01, SignOff: null,        Month: March  → 31 days (full month)
        ///   SignOn: 2025-04-01, SignOff: null,        Month: March  → 0  days (not started)
        /// </summary>
        public int GetWorkingDays(CrewContract contract, DateOnly month)
        {
            if (contract is null) throw new ArgumentNullException(nameof(contract));

            var firstDay = new DateOnly(month.Year, month.Month, 1);
            var lastDay  = new DateOnly(month.Year, month.Month,
                                        DateTime.DaysInMonth(month.Year, month.Month));

            // Contract hasn't started yet this month
            if (contract.SignOnDate > lastDay)  return 0;

            // Contract ended before this month
            if (contract.SignOffDate.HasValue && contract.SignOffDate.Value < firstDay) return 0;

            // Clamp start and end to month boundaries
            var effectiveStart = contract.SignOnDate  > firstDay ? contract.SignOnDate : firstDay;
            var effectiveEnd   = contract.SignOffDate.HasValue && contract.SignOffDate.Value < lastDay
                                     ? contract.SignOffDate.Value
                                     : lastDay;

            return effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
        }
    }
}
