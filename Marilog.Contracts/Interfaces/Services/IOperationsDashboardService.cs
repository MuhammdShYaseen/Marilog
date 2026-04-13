using Marilog.Contracts.DTOs.Responses;


namespace Marilog.Contracts.Interfaces.Services
{
    public interface IOperationsDashboardService
    {
        Task<OperationsDashboardResponse>GetAsync(CancellationToken ct = default);
    }
}
