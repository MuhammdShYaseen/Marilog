using Marilog.Contracts.DTOs.Reports.PaymentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Globalization;
using System.Net.Http.Json;


namespace Marilog.Client.Services.SystemServices
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _http;
        private const string Base = "api/documents";

        public PaymentService(HttpClient http)
        {
            _http = http;
        }
        public async Task<PaymentResponse> AddPaymentAsync(int documentId, AddPaymentRequest create, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/payments", create, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<PaymentResponse>(ct);
            return response!;
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(int documentId, int paymentId, UpdatePaymentRequest update, CancellationToken ct = default)
        {

            var http = await _http.PutAsJsonAsync($"{Base}/{documentId}/payments/{paymentId}", update, ct);

            await http.EnsureSuccessWithMessageAsync(ct);

            var response = await http.Content.ReadFromJsonAsync<PaymentResponse>(ct);

            return response!;
        }

        public async Task RemovePaymentAsync(int documentId, int paymentId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{documentId}/payment/{paymentId}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task<PaymentsReport> GetPaymentsReportAsync(FilterPaymentOptionsRequest options, CancellationToken ct = default)
        {
            var query = BuildPaymentQueryString(options);
            var response = await _http.GetApiAsync<PaymentsReport>($"api/reports/payments{query}", ct) ?? new PaymentsReport();
            return response;
        }



        //====Privates==============================================
        private static string BuildPaymentQueryString(FilterPaymentOptionsRequest options)
        {
            var parts = new List<string>();

            if (options.SupplierId.HasValue)
                parts.Add($"supplierId={options.SupplierId.Value}");

            if (options.BuyerId.HasValue)
                parts.Add($"buyerId={options.BuyerId.Value}");

            if (options.VesselId.HasValue)
                parts.Add($"vesselId={options.VesselId.Value}");

            if (options.DocTypeId.HasValue)
                parts.Add($"docTypeId={options.DocTypeId.Value}");

            if (options.LastDays.HasValue)
                parts.Add($"lastDays={options.LastDays.Value}");

            if (options.FromDate.HasValue)
                parts.Add($"fromDate={options.FromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (options.ToDate.HasValue)
                parts.Add($"toDate={options.ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (options.Side.HasValue)
                parts.Add($"side={options.Side.Value}");

            if (options.PaymentMethod.HasValue)
                parts.Add($"paymentMethod={options.PaymentMethod.Value}");

            if (options.SwiftOnly.HasValue)
                parts.Add($"swiftOnly={options.SwiftOnly.Value}");

            if (options.MinAmount.HasValue)
                parts.Add($"minAmount={options.MinAmount.Value.ToString(CultureInfo.InvariantCulture)}");

            if (options.MaxAmount.HasValue)
                parts.Add($"maxAmount={options.MaxAmount.Value.ToString(CultureInfo.InvariantCulture)}");

            if (options.VoyageId.HasValue)
                parts.Add($"voyageId={options.VoyageId.Value}");

            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }
    }
}
