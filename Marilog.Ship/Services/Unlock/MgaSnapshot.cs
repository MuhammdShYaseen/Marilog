using System.Text.Json;
using Marilog.Ship.Domain.Entities;

namespace Marilog.Ship.Services.Unlock
{
    /// <summary>JSON copy of a closed MGA, kept in UnlockLog before it is reopened and after it is closed again.</summary>
    internal static class MgaSnapshot
    {
        public static string ToJson(Mga mga) => JsonSerializer.Serialize(new
        {
            mga.Number,
            mga.Code,
            mga.FromDate,
            mga.ToDate,
            mga.MasterName,
            mga.EndReason,
            mga.Notes,
            mga.PreviousBalance,
            mga.TotalWages,
            mga.TotalExpenses,
            mga.OfficeDeductions,
            mga.TotalReceived,
            mga.ExchangeDifference,
            mga.CountDifference,
            mga.ClosingBalance,
            mga.ExpectedCigarettes,
            mga.CountedCigarettes,
            mga.ClosedAt,
            Crew = mga.CrewLines.Select(x => new
            {
                x.CrewContractId,
                x.WagesPaidBy,
                x.MonthlyWage,
                x.Days,
                x.PreviousBalance,
                x.Wages,
                x.Extras,
                x.Cash,
                x.Cigarettes,
                x.Other,
                x.PaidAshore,
                x.Balance
            }),
            Cash = mga.CashLines.Select(x => new { x.CurrencyId, x.Expected, x.Counted, x.RateToBase })
        });
    }
}