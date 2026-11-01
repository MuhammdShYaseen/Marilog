using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.InboxEntryReport;
using Marilog.Contracts.DTOs.Requests.InboxEntryDTOs;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IInboxService
    {
        Task<string> CreateAsync(DateOnly receivedDate, InboxSource source, List<int> vesselId, string description, int? sequence = null, CancellationToken ct = default);
        Task<List<string>> CreateRangeAsync(List<CreateInboxEntryItem> items, bool keepInputOrder = false, CancellationToken ct = default);
        Task UpdateAsync(int id, DateOnly receivedDate, InboxSource source, List<int> vesselIds, string description, CancellationToken ct = default);
        Task<string> ChangeStartAsync(int id, int sequence, CancellationToken ct = default);
        Task LinkAsync(int id, EntityType entityType, int entityId, CancellationToken ct = default);
        Task UnlinkAsync(int linkId, CancellationToken ct = default);
        Task MarkProcessedAsync(int id, CancellationToken ct = default);
        Task CancelAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<PagedResponse<InboxEntryResponse>> GetFilteredAsync(InboxEntryFilter filter, CancellationToken ct = default);
        Task<PagedResponse<InboxEntryResponse>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);
    }
}
