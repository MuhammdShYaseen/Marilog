using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Frontend;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.FrontendServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
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

        [HttpPost]
        public async Task<ActionResult<NavItemResponse>> Create([FromBody] CreateNavItemRequest request, CancellationToken ct)
        {
            var NavItem = await _service.CreateAsync(request.Title, request.Route, request.Icon, request.ParentId, request.Order, ct);

            return Ok(ApiResponse<NavItemResponse>.Ok(NavItem));
        }

        [HttpPost("batch")]
        [ProducesResponseType(typeof(ApiResponse<List<NavItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRangeAsync(
        [FromBody] List<CreateNavItemRequest> items,
        CancellationToken ct)
        {
            var result = await _service.CreateRangeAsync(items, ct);

            return Ok(
                ApiResponse<List<NavItemResponse>>
                    .Ok(result, "Nav items created successfully"));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<NavItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateNavItemRequest request,
        CancellationToken ct)
        {
            var result = await _service.UpdateAsync(
                id,
                request.Title,
                request.Route,
                request.Icon,
                request.ParentId,
                request.Order,
                ct);

            return Ok(
                ApiResponse<NavItemResponse>
                    .Ok(result, "Nav item updated successfully"));
        }

        // =========================================
        // DELETE
        // DELETE: api/navitems/{id}
        // =========================================

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAsync(
            int id,
            CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);

            return Ok(
                ApiResponse<string>
                    .Ok("Deleted", "Nav item deleted successfully"));
        }
    }
}
