using Marilog.Domain.Entities.SystemEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Services
{
    public interface IPayrollCalculatorService
    {
        decimal CalculateBasicWage(decimal monthlyWage, int workingDays,
            int totalDaysInMonth);

        int GetWorkingDays(CrewContract contract, DateOnly month);
    }
}
