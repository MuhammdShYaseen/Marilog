using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Presentation.DTOs.DocumentTypeDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
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
            return docType is null ? NotFound() : Ok(docType);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DocumentTypeResponse>>> GetAll(CancellationToken ct)
        {
            var list = await _service.GetAllAsync(ct);
            return Ok(list);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<DocumentTypeResponse>>> GetActive(CancellationToken ct)
        {
            var list = await _service.GetActiveAsync(ct);
            return Ok(list);
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

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
