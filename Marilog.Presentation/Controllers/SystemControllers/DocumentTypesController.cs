using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.DocumentTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/document-types")]
    public class DocumentTypesController : ControllerBase
    {
        private readonly IDocumentTypeService _service;

        public DocumentTypesController(IDocumentTypeService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DocumentTypeResponse>> GetById(int id, CancellationToken ct)
        {
            var docType = await _service.GetByIdAsync(id, ct);
            return docType is null ? NotFound() : Ok(docType);
        }

        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<DocumentTypeResponse>> GetByCode(string code, CancellationToken ct)
        {
            var docType = await _service.GetByCodeAsync(code, ct);
            return docType is null ? NotFound() : Ok(ApiResponse<DocumentTypeResponse>.Ok(docType));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DocumentTypeResponse>>> GetAll(CancellationToken ct)
        {
            var list = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<DocumentTypeResponse>>.Ok(list));
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<DocumentTypeResponse>>> GetActive(CancellationToken ct)
        {
            var list = await _service.GetActiveAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<DocumentTypeResponse>>.Ok(list));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<DocumentTypeResponse>> Create(
            [FromBody] CreateDocumentTypeRequest request,
            CancellationToken ct)
        {
            var result = await _service.CreateAsync(request.Code, request.Name, request.SortOrder, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DocumentTypeResponse>.Ok(result));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentTypeResponse>>>> CreateRange(
        [FromBody] IEnumerable<CreateDocumentTypeRequest> requests,
         CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateDocumentTypeRequest
            {
                Code = r.Code,
                Name = r.Name,
                SortOrder = r.SortOrder
            });

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<DocumentTypeResponse>>.Ok(result));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateDocumentTypeRequest request,
            CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.Name, request.SortOrder, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
