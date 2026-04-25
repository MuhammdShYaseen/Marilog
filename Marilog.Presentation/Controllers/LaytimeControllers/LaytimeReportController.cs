using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.LaytimeControllers
{
    [ApiController]
    [Route("api/laytime-reports")]
    public sealed class LaytimeReportController : ControllerBase
    {
        private readonly ILaytimeReportService _service;

        public LaytimeReportController(ILaytimeReportService service)
            => _service = service;

        [HttpGet("calculations/{calculationId:int}/summary")]
        public async Task<IActionResult> Summary(
            int calculationId,
            [FromQuery] ReportFormat format,
            CancellationToken cancellationToken)
        {
            var bytes = await _service.GenerateSummaryReportAsync(
                calculationId,
                format,
                cancellationToken);

            return File(
                bytes,
                GetContentType(format),
                $"laytime-summary-{calculationId}.{GetExtension(format)}");
        }

        [HttpGet("calculations/{calculationId:int}/detailed")]
        public async Task<IActionResult> Detailed(
            int calculationId,
            [FromQuery] ReportFormat format,
            CancellationToken cancellationToken)
        {
            var bytes = await _service.GenerateDetailedReportAsync(
                calculationId,
                format,
                cancellationToken);

            return File(
                bytes,
                GetContentType(format),
                $"laytime-detailed-{calculationId}.{GetExtension(format)}");
        }

        [HttpGet("calculations/{calculationId:int}/delays")]
        public async Task<IActionResult> Delays(
            int calculationId,
            [FromQuery] ReportFormat format,
            CancellationToken cancellationToken)
        {
            var bytes = await _service.GenerateDelayReportAsync(
                calculationId,
                format,
                cancellationToken);

            return File(
                bytes,
                GetContentType(format),
                $"laytime-delays-{calculationId}.{GetExtension(format)}");
        }

        [HttpGet("contracts/{contractId:int}/laytime")]
        public async Task<IActionResult> ContractLaytime(
            int contractId,
            [FromQuery] ReportFormat format,
            CancellationToken cancellationToken)
        {
            var bytes = await _service.GenerateContractLaytimeReportAsync(
                contractId,
                format,
                cancellationToken);

            return File(
                bytes,
                GetContentType(format),
                $"contract-laytime-{contractId}.{GetExtension(format)}");
        }

        [HttpGet("calculations/{calculationId:int}/timesheet-excel")]
        public async Task<IActionResult> TimeSheetExcel(
            int calculationId,
            CancellationToken cancellationToken)
        {
            var bytes = await _service.GenerateTimeSheetExcelAsync(
                calculationId,
                cancellationToken);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"timesheet-{calculationId}.xlsx");
        }

        private static string GetContentType(ReportFormat format)
            => format switch
            {
                ReportFormat.Pdf =>
                    "application/pdf",

                ReportFormat.Excel =>
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                _ => "application/octet-stream"
            };

        private static string GetExtension(ReportFormat format)
            => format switch
            {
                ReportFormat.Pdf => "pdf",
                ReportFormat.Excel => "xlsx",
                _ => "bin"
            };
    }
}
