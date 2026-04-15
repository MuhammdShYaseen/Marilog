using Marilog.Domain.Entities.SystemEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Services
{
    /// <summary>
    /// Domain service — encapsulates payroll calculation logic that
    /// spans CrewContract + Calendar and doesn't belong to a single entity.
    /// </summary>
    public interface IPayrollCalculatorService
    {
        /// <summary>
        /// Calculates prorated BasicWage based on actual working days in the month.
        /// </summary>
        decimal CalculateBasicWage(decimal monthlyWage, int workingDays, int totalDaysInMonth);

        /// <summary>
        /// Returns the number of days the crew member served during the given month,
        /// considering SignOnDate and SignOffDate boundaries.
        /// </summary>
        int GetWorkingDays(CrewContract contract, DateOnly month);
    }
}
