using Marilog.Contracts.DTOs.Responses;


namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IOperationsDashboardService
    {
        Task<OperationsDashboardResponse>GetAsync(CancellationToken ct = default);
    }
}
