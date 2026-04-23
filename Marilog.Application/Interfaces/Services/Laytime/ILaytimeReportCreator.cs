using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Domain.Entities.LaytimeEntities;

namespace Marilog.Application.Interfaces.Services.Laytime
{
    public interface ILaytimeReportCreator
    {
        Task<byte[]> GenerateTimeSheetExcelAsync(
     LaytimeCalculation calculation,
     CharterTerms charterTerms,
     CancellationToken cancellationToken = default);

        Task<byte[]> GenerateSummaryReportAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default);

        Task<byte[]> GenerateDetailedReportAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default);

        Task<byte[]> GenerateDelayReportAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default);

        Task<byte[]> GenerateContractLaytimeReportAsync(
            IReadOnlyList<LaytimeCalculation> calculations,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default);
    }
}
