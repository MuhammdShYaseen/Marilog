using System.Text.Json;
using Marilog.Ship.Domain.Entities;

namespace Marilog.Ship.Services.Unlock
{
    /// <summary>JSON copy of the opening balances, kept in UnlockLog before and after an edit.</summary>
    internal static class OpeningSnapshot
    {
        public static string ToJson(Mga opening) => JsonSerializer.Serialize(new
        {
            AsOf = opening.FromDate,
            opening.MasterName,
            MasterAccountBalance = opening.ClosingBalance,
            CigarettesOnBoard = opening.ExpectedCigarettes,
            opening.Notes,
            Crew = opening.CrewLines.Select(x => new { x.CrewContractId, x.Balance }),
            Cash = opening.CashLines.Select(x => new { x.CurrencyId, Amount = x.Expected })
        });
    }
}