using Marilog.Application.DTOs.LogDTOs;
using Marilog.Application.DTOs.Logs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.LogInterfaces
{
    public interface ILogReaderService
    {
        Task<LogReadResult> QueryAsync(LogQuery query, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetAvailableFilesAsync(CancellationToken ct = default);
    }

}
