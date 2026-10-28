

using Marilog.Contracts.DTOs.Reports.PaymentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IPaymentService
    {

        // ── Payments ──────────────────────────────────────────────────────────────
        Task<PaymentResponse> AddPaymentAsync(int documentId, AddPaymentRequest create, CancellationToken ct = default);
        Task<PaymentResponse> UpdatePaymentAsync(int documentId, int paymentId, UpdatePaymentRequest update, CancellationToken ct = default);
        Task RemovePaymentAsync(int documentId, int paymentId, CancellationToken ct = default);
        Task<PaymentsReport> GetPaymentsReportAsync(FilterPaymentOptionsRequest options, CancellationToken ct = default);
    }
}
