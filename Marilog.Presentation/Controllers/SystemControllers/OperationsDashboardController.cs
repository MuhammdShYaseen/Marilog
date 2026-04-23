using Marilog.Contracts.Common;
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
    }
}
