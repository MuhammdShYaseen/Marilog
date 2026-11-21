using Marilog.Application.Interfaces.DocumentTemplates;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    // Permissions come from PermissionConvention (DocumentTemplate, open read).
    // Export and preview read a document's data, so they need Read on Document instead.
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentTemplatesController : ControllerBase
    {
        private readonly IDocumentTemplateService _service;
        private readonly IDocumentTemplateRenderer _renderer;

        public DocumentTemplatesController(IDocumentTemplateService service, IDocumentTemplateRenderer renderer)
        {
            _service = service;
            _renderer = renderer;
        }

        // ── Queries ──────────────────────────────────────────────

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentTemplateSummaryResponse>>>> GetAll(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<DocumentTemplateSummaryResponse>>.Ok(await _service.GetAllAsync(ct)));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<DocumentTemplateResponse>>> GetById(int id, CancellationToken ct)
        {
            var template = await _service.GetByIdAsync(id, ct);
            if (template is null)
                return NotFound(ApiResponse<DocumentTemplateResponse>.Fail($"Document template with ID {id} was not found."));

            return Ok(ApiResponse<DocumentTemplateResponse>.Ok(template));
        }

        [HttpGet("fonts")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<TemplateFontResponse>>>> GetFonts(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<TemplateFontResponse>>.Ok(await _service.GetFontsAsync(ct)));

        // The browser loads fonts through CSS @font-face, which sends no cookie to another origin.
        // The files are the open-licence fonts shipped with Marilog, so they are public.
        [AllowAnonymous]
        [HttpGet("fonts/{fileName}")]
        public IActionResult GetFontFile(string fileName)
        {
            var path = _renderer.GetFontFilePath(fileName);
            if (path is null) return NotFound();

            var contentType = Path.GetExtension(path).Equals(".otf", StringComparison.OrdinalIgnoreCase) ? "font/otf" : "font/ttf";
            return PhysicalFile(path, contentType);
        }

        // ── Commands ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DocumentTemplateResponse>>> Create(
            [FromBody] SaveDocumentTemplateRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<DocumentTemplateResponse>.Ok(result, "Template created successfully."));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<DocumentTemplateResponse>>> Update(
            int id, [FromBody] SaveDocumentTemplateRequest request, CancellationToken ct)
            => Ok(ApiResponse<DocumentTemplateResponse>.Ok(await _service.UpdateAsync(id, request, ct), "Template updated successfully."));

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return Ok(ApiResponse<bool>.Ok(true, "Template deleted successfully."));
        }

        // ── PDF ──────────────────────────────────────────────────

        [RequirePermission(EntityType.Document, PermissionAction.Read)]
        [HttpGet("{id:int}/export/{documentId:int}")]
        public async Task<IActionResult> Export(int id, int documentId, CancellationToken ct)
        {
            var pdf = await _service.ExportAsync(id, documentId, ct);
            return File(pdf, "application/pdf", $"document_{documentId}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        [RequirePermission(EntityType.Document, PermissionAction.Read)]
        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] PreviewDocumentTemplateRequest request, CancellationToken ct)
        {
            var pdf = await _service.PreviewAsync(request, ct);
            return File(pdf, "application/pdf", "template_preview.pdf");
        }
    }
}