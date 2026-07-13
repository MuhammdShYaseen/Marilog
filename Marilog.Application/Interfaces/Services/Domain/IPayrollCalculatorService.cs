using Marilog.Domain.Entities.SystemEntities;


namespace Marilog.Application.Interfaces.Services.Domain
{
    public interface IPayrollCalculatorService
    {
        decimal CalculateBasicWage(decimal monthlyWage, int workingDays,
            int totalDaysInMonth);

        int GetWorkingDays(CrewContract contract, DateOnly month);
    }
}
