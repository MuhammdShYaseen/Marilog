using Marilog.Application.DTOs.LogDTOs;

namespace Marilog.Application.Interfaces.LogService
{
    public interface ILogReaderService
    {
        Task<LogReadResult> QueryAsync(LogQuery query, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetAvailableFilesAsync(CancellationToken ct = default);
        Task DeleteFileAsync(string fileName, CancellationToken ct = default);
        Task<int> DeleteOldFilesAsync(CancellationToken ct = default);
    }

}
