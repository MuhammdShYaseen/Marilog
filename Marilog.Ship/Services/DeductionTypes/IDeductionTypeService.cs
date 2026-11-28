namespace Marilog.Ship.Services.DeductionTypes
{
    /// <summary>
    /// Lookup for "Other" crew deductions. No delete: old withdrawals keep their type;
    /// a type that is no longer used is deactivated.
    /// </summary>
    public interface IDeductionTypeService
    {
        Task<IReadOnlyList<DeductionTypeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<DeductionTypeResponse>> GetActiveAsync(CancellationToken ct = default);
        Task CreateAsync(string code, string name, CancellationToken ct = default);
        Task UpdateAsync(int id, string name, CancellationToken ct = default);
        Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
    }
}