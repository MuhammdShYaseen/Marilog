using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IStoredFileQueryService
    {
        Task<StoredFileResponse?> GetByIdAsync(int id);

        Task<IReadOnlyList<StoredFileResponse>> GetByEntityIdAsync(int entityId, EntityType entityType);

        Task<PagedResponse<StoredFileResponse>> FullTextSearchAsync(string query, int page, int pageSize, EntityType? entityType = null);

        Task<Stream> GetFileStreamAsync(int id);
    }
}
