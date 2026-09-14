using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/operations/dashboard")]
    public class OperationsDashboardController
         : ControllerBase
    {
        private readonly IOperationsDashboardService _service;

        public OperationsDashboardController(IOperationsDashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<OperationsDashboardResponse>>>Get(CancellationToken ct)
        {
            var data = await _service.GetAsync(ct);

            return Ok(ApiResponse<OperationsDashboardResponse>.Ok(data));
        }

        [HttpGet("financial-summary")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<FinancialChartPointResponse>>>> GetFinancialSummary(
        [FromQuery] DocumentFilterOptions options, CancellationToken ct)
        {
            var data = await _service.GetFinancialSummaryAsync(options, ct);
            return Ok(ApiResponse<IReadOnlyList<FinancialChartPointResponse>>.Ok(data));
        }

        [HttpGet("voyage-summary")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<VoyageChartPointResponse>>>> GetVoyageSummary(
            [FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int? vesselId, CancellationToken ct)
        {
            var data = await _service.GetVoyageSummaryAsync(from, to, vesselId, ct);
            return Ok(ApiResponse<IReadOnlyList<VoyageChartPointResponse>>.Ok(data));
        }
    }
}
