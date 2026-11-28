namespace Marilog.Ship.Services.GeneralAccount
{
    /// <summary>
    /// Master's General Account. The open period is computed live from the records;
    /// closing saves a snapshot and locks every record dated inside it.
    /// Wages use CrewWageRules — the same rule as Marilog payroll.
    /// </summary>
    public interface IMgaService
    {
        /// <summary>Closed MGAs, newest first (the opening balances are not listed).</summary>
        Task<IReadOnlyList<MgaListItem>> GetClosedAsync(CancellationToken ct = default);

        /// <summary>Null until the initial setup is completed.</summary>
        Task<MgaOpenPeriod?> GetOpenPeriodAsync(CancellationToken ct = default);

        /// <summary>The open period up to 'to', computed live with today's rates.</summary>
        Task<MgaReport> GetDraftAsync(DateOnly to, string? endReason, CancellationToken ct = default);

        Task<MgaReport> GetReportAsync(int mgaId, CancellationToken ct = default);

        /// <summary>Physical count is mandatory. A difference needs a reason.</summary>
        Task CloseAsync(CloseMgaRequest request, CancellationToken ct = default);

        /// <summary>Master-paid crew with their balance as at 'asOf' and the wage of 'wageMonth'.</summary>
        Task<IReadOnlyList<CrewPayAshoreItem>> GetPayAshoreCrewAsync(DateOnly asOf, DateOnly wageMonth,
            CancellationToken ct = default);
    }
}