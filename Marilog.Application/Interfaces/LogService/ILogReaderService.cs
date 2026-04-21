using Marilog.Application.DTOs.LogDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.LogService
{
    public interface ILogReaderService
    {
        Task<LogReadResult> QueryAsync(LogQuery query, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetAvailableFilesAsync(CancellationToken ct = default);
    }

}
