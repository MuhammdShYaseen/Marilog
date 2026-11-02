using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs;

namespace Marilog.Web.Client.Services.Interfaces
{
    public interface IRecordCreator
    {
        Task<int?> CreateDocumentAsync(CreateDocumentRequest? prefill = null, CancellationToken ct = default);
        Task<int?> CreateSwiftTransferAsync(CreateSwiftTransferRequest? prefill = null, CancellationToken ct = default);
        Task<int?> CreatePaymentAsync(int documentId, CancellationToken ct = default);
        Task<int?> CreateAdjustmentAsync(int documentId, CancellationToken ct = default);
    }
}
