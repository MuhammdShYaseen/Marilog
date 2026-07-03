using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.FunctionaltyServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.FunctionaltyControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfFileGeneratorController : ControllerBase
    {
        private readonly IPdfFileGeneratorService _pdfGeneratorService;
        public PdfFileGeneratorController(IPdfFileGeneratorService pdfFileGenerator)
        {
            _pdfGeneratorService = pdfFileGenerator;
        }
        [HttpPost("DocumentReport")]
        public async Task<IActionResult> ExportDocumentReportPdf([FromBody] DocumentReport report,[FromQuery] string? title = null, CancellationToken ct = default)
        {
            var pdfTitle = title ?? "Document Report";
            var pdfBytes =await _pdfGeneratorService.GenerateDocumentReportPdf(report, pdfTitle, ct);
            var fileName = $"DocumentReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        [HttpPost("BillOfLading")]
        public async Task<IActionResult> ExportBillOfLadingFile([FromBody] BillOfLadingResponse bl, CancellationToken ct = default)
        {
            var pdfBytes = await _pdfGeneratorService.GenerateBillOfLadingFile(bl, ct);
            var fileName = $"billOfLading_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

    }
}
