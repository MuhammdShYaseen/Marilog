using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.AiProviderDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.AiProviderServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.AiControllers
{
    // AiProvider is in PermissionCatalog.SuperAdminOnly: PermissionConvention makes every action super admin only.
    [ApiController]
    [Route("api/ai-providers")]
    public sealed class AiProvidersController : ControllerBase
    {
        private readonly IAiProviderService _service;

        public AiProvidersController(IAiProviderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AiProviderResponse>>>> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IEnumerable<AiProviderResponse>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AiProviderResponse>>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result is null)
                return NotFound(ApiResponse<AiProviderResponse>.Fail("Not found"));

            return Ok(ApiResponse<AiProviderResponse>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AiProviderResponse>>> Create(CreateAiProviderRequest request, CancellationToken ct)
        {
            var response = await _service.CreateAsync(request, ct);
            return Ok(ApiResponse<AiProviderResponse>.Ok(response));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, UpdateAiProviderRequest request, CancellationToken ct)
        {
            if (!await _service.UpdateAsync(id, request, ct))
                return NotFound(ApiResponse<bool>.Fail("Not found"));

            return Ok(ApiResponse<bool>.Ok(true));
        }

        [HttpPut("{id:int}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> Activate(int id, CancellationToken ct)
        {
            if (!await _service.ActivateAsync(id, ct))
                return NotFound(ApiResponse<bool>.Fail("Not found"));

            return Ok(ApiResponse<bool>.Ok(true));
        }

        [HttpPut("{id:int}/deactivate")]
        public async Task<ActionResult<ApiResponse<bool>>> Deactivate(int id, CancellationToken ct)
        {
            if (!await _service.DeactivateAsync(id, ct))
                return NotFound(ApiResponse<bool>.Fail("Not found"));

            return Ok(ApiResponse<bool>.Ok(true));
        }

        // Sends one prompt to this provider (active or not) and returns the answer text.
        [HttpPost("{id:int}/test")]
        public async Task<ActionResult<ApiResponse<string>>> Test(int id, TestAiProviderRequest request, CancellationToken ct)
        {
            var text = await _service.TestAsync(id, request.Prompt, ct);
            return Ok(ApiResponse<string>.Ok(text));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken ct)
        {
            if (!await _service.DeleteAsync(id, ct))
                return NotFound(ApiResponse<bool>.Fail("Not found"));

            return Ok(ApiResponse<bool>.Ok(true));
        }
    }
}