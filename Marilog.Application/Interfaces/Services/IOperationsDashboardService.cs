using Marilog.Application.DTOs.Responses;


namespace Marilog.Application.Interfaces.Services
{
    public interface IOperationsDashboardService
    {
        Task<OperationsDashboardResponse>GetAsync(CancellationToken ct = default);
    }
}
