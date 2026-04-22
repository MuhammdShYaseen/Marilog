using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;

namespace Marilog.Contracts.Interfaces.Services.CharterLaytimeServices
{
    public interface ILaytimeReportService
    {
        /// <summary>
        /// تقرير ملخص — Summary Report (PDF أو Excel).
        /// يتضمن: Allowed vs Used، النتيجة النهائية، Demurrage/Despatch.
        /// </summary>
        Task<byte[]> GenerateSummaryReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default);

        /// <summary>
        /// تقرير تفصيلي — Detailed Report (PDF أو Excel).
        /// يتضمن: كل SOF Events، كل Segments، كل Exceptions، الحسابات كاملة.
        /// </summary>
        Task<byte[]> GenerateDetailedReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default);

        /// <summary>
        /// تقرير التأخيرات — Delay Report (PDF أو Excel).
        /// يتضمن: فترات التأخير فقط، أسبابها، مدتها، تأثيرها على الحساب.
        /// </summary>
        Task<byte[]> GenerateDelayReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default);

        /// <summary>
        /// تقرير مقارنة Allowed vs Used لعقد كامل (جميع Calculations).
        /// مفيد لمتابعة أداء الأسطول عبر الرحلات.
        /// </summary>
        Task<byte[]> GenerateContractLaytimeReportAsync(int contractId, ReportFormat format, CancellationToken cancellationToken = default);
    }
}
