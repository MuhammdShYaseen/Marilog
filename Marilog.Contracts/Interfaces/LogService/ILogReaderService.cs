using Marilog.Contracts.DTOs.LogDTOs;

namespace Marilog.Contracts.Interfaces.LogService
{
    public interface ILogReaderService
    {
        Task<LogReadResult> QueryAsync(LogQuery query, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetAvailableFilesAsync(CancellationToken ct = default);
    }

}
