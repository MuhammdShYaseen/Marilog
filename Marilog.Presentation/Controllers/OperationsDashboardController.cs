using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
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
    }
}
