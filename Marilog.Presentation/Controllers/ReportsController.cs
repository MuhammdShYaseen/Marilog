using Marilog.Application.DTOs.Reports.CrewContractReports;
using Marilog.Application.DTOs.Reports.CrewPayrollReports;
using Marilog.Application.DTOs.Reports.DocumentReports;
using Marilog.Application.DTOs.Reports.SwiftTransferReports;
using Marilog.Application.DTOs.Reports.VoyageReports;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Produces("application/json")]
    public sealed class ReportsController : ControllerBase
    {
        private readonly ISwiftTransferService _swiftService;
        private readonly IVoyageService _voyageService;
        private readonly ICrewPayrollService _payrollService;
        private readonly ICrewContractService _contractService;
        private readonly IDocumentService _documentService;

        public ReportsController(
            ISwiftTransferService swiftService,
            IVoyageService voyageService,
            ICrewPayrollService payrollService,
            ICrewContractService contractService,
            IDocumentService documentService)
        {
            _swiftService = swiftService;
            _voyageService = voyageService;
            _payrollService = payrollService;
            _contractService = contractService;
            _documentService = documentService;
        }

       
        [HttpGet("swift-transfers")]
        [ProducesResponseType(typeof(ApiResponse<SwiftTransferReport>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSwiftTransfersReport([FromQuery] SwiftTransferFilterOptions filterOptions,
            CancellationToken ct)
        {
            var report = await _swiftService.GetSwiftTransfersReportAsync(filterOptions, ct);
            return Ok(ApiResponse<SwiftTransferReport>.Ok(report));
        }

        
        [HttpGet("voyages")]
        [ProducesResponseType(typeof(ApiResponse<VoyageReport>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVoyagesReport([FromQuery] VoyageReportFilterOptions filterOptions,
            CancellationToken ct)
        {
            var report = await _voyageService.GetVoyagesReportAsync(filterOptions, ct);
            return Ok(ApiResponse<VoyageReport>.Ok(report));
        }

        [HttpGet("crew-payroll")]
        [ProducesResponseType(typeof(ApiResponse<CrewPayrollReport>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCrewPayrollReport([FromQuery] CrewPayrollFilterOptions filterOptions,
            CancellationToken ct)
        {
            var report = await _payrollService.GetCrewPayrollReportAsync(filterOptions, ct);
            return Ok(ApiResponse<CrewPayrollReport>.Ok(report));
        }

        [HttpGet("crew-contracts")]
        [ProducesResponseType(typeof(ApiResponse<CrewContractReport>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCrewContractsReport([FromQuery] CrewContractFilterOptions filterOptions,
            CancellationToken ct)
        {
            var report = await _contractService.GetCrewContractsReportAsync(filterOptions, ct);
            return Ok(ApiResponse<CrewContractReport>.Ok(report));
        }

       
        [HttpGet("documents")]
        [ProducesResponseType(typeof(ApiResponse<DocumentReport>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentsReport([FromQuery] DocumentFilterOptions filterOptions,
            CancellationToken ct)
        {
            var report = await _documentService.GetFilteredDocsReportAsync(filterOptions, ct);
            return Ok(ApiResponse<DocumentReport>.Ok(report));
        }
    }
}
