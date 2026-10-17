using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Frontend.NavItem;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.FrontendServices;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Marilog.Presentation.Controllers.Frontend
{
    [ApiController]
    [Route("api/navigation")]
    public class NavigationController : ControllerBase
    {
        private readonly INavigationService _service;
        private static readonly JsonSerializerOptions ExportJson = new(JsonSerializerDefaults.Web) { WriteIndented = true };
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



        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<NavItemResponse>>>> GetAll(CancellationToken ct)
        {
            var data = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<List<NavItemResponse>>.Ok(data));
        }

        [HttpPut("layout")]
        public async Task<IActionResult> SaveLayoutAsync([FromBody] List<NavItemLayoutRequest> items, CancellationToken ct)
        {
            await _service.SaveLayoutAsync(items, ct);
            return Ok(ApiResponse<string>.Ok("Saved", "Navigation layout saved"));
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> ActivateAsync(int id, CancellationToken ct)
        {
            await _service.SetActiveAsync(id, true, ct);
            return Ok(ApiResponse<string>.Ok("Activated", "Nav item activated"));
        }

        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateAsync(int id, CancellationToken ct)
        {
            await _service.SetActiveAsync(id, false, ct);
            return Ok(ApiResponse<string>.Ok("Deactivated", "Nav item deactivated"));
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportAsync(CancellationToken ct)
        {
            var data = await _service.ExportAsync(ct);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(data, ExportJson);
            return File(bytes, "application/json", $"marilog-navigation-{DateTime.UtcNow:yyyyMMdd}.json");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportAsync([FromBody] List<NavItemExport> items, CancellationToken ct)
        {
            await _service.ImportAsync(items, ct);
            return Ok(ApiResponse<string>.Ok("Imported", "Navigation imported"));
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
