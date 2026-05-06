using Marilog.Contracts.DTOs.Reports.CrewContractReports;
using Marilog.Contracts.DTOs.Reports.CrewPayrollReports;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Reports.SwiftTransferReports;
using Marilog.Contracts.DTOs.Reports.VoyageReports;

namespace Marilog.Client.Interfaces
{
    public interface IReportsService
    {
        Task<SwiftTransferReport> GetSwiftTransfersReportAsync(SwiftTransferFilterOptions filter, CancellationToken ct = default);
        Task<VoyageReport> GetVoyagesReportAsync(VoyageReportFilterOptions filter, CancellationToken ct = default);
        Task<CrewPayrollReport> GetCrewPayrollReportAsync(CrewPayrollFilterOptions filter, CancellationToken ct = default);
        Task<CrewContractReport> GetCrewContractsReportAsync(CrewContractFilterOptions filter, CancellationToken ct = default);
        Task<DocumentReport> GetDocumentsReportAsync(DocumentFilterOptions filter, CancellationToken ct = default);
    }
}
