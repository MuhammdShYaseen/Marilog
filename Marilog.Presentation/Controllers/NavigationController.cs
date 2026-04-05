using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.FrontendServices;
using Marilog.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/navigation")]
    public class NavigationController : ControllerBase
    {
        private readonly INavigationService _service;

        public NavigationController(INavigationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NavItemResponse>>>> Get(CancellationToken ct)
        {
            var data = await _service.GetAsync(ct);
            return Ok(ApiResponse<List<NavItemResponse>>.Ok(data));
        }
    }
}
