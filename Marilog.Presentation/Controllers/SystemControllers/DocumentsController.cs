using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{



    [ApiController]
    [Route("api/documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _service;

        public DocumentsController(IDocumentService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DocumentResponse>> GetById(int id, CancellationToken ct)
        {
            var doc = await _service.GetByIdAsync(id, ct);
            return doc is null ? NotFound() : Ok(ApiResponse<DocumentResponse>.Ok(doc));
        }

        [HttpGet("{id:int}/with-items")]
        public async Task<ActionResult<DocumentResponse>> GetWithItems(int id, CancellationToken ct)
        {
            var doc = await _service.GetWithItemsAsync(id, ct);
            return doc is null ? NotFound() : Ok(ApiResponse<DocumentResponse>.Ok(doc));
        }

        [HttpGet("{id:int}/with-payments")]
        public async Task<ActionResult<DocumentResponse>> GetWithPayments(int id, CancellationToken ct)
        {
            var doc = await _service.GetWithPaymentsAsync(id, ct);
            return doc is null ? NotFound() : Ok(ApiResponse<DocumentResponse>.Ok(doc));
        }

        [HttpGet("{id:int}/full")]
        public async Task<ActionResult<DocumentResponse>> GetFull(int id, CancellationToken ct)
        {
            var doc = await _service.GetFullAsync(id, ct);
            return doc is null ? NotFound() : Ok(ApiResponse<DocumentResponse>.Ok(doc));
        }

        [HttpGet("by-number/{docNumber}")]
        public async Task<ActionResult<DocumentResponse>> GetByNumber(string docNumber, CancellationToken ct)
        {
            var doc = await _service.GetByNumberAsync(docNumber, ct);
            return doc is null ? NotFound() : Ok(ApiResponse<DocumentResponse>.Ok(doc));
        }

        [HttpGet("by-supplier/{supplierId:int}")]
        public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> GetBySupplier(int supplierId, CancellationToken ct)
        {
            var docs = await _service.GetBySupplierAsync(supplierId, ct);
            return docs is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(docs));
        }

        [HttpGet("by-buyer/{buyerId:int}")]
        public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> GetByBuyer(int buyerId, CancellationToken ct)
        {
            var docs = await _service.GetByBuyerAsync(buyerId, ct);
            return docs is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(docs));
        }

        [HttpGet("by-vessel/{vesselId:int}")]
        public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> GetByVessel(int vesselId, CancellationToken ct)
        {
            var docs = await _service.GetByVesselAsync(vesselId, ct);
            return docs is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(docs));
        }

        [HttpGet("by-type/{docTypeId:int}")]
        public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> GetByType(int docTypeId, CancellationToken ct)
        {
            var docs = await _service.GetByTypeAsync(docTypeId, ct);
            return docs is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(docs));
        }

        [HttpGet("unpaid")]
        public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> GetUnpaid(CancellationToken ct)
        {
            var docs = await _service.GetUnpaidAsync(ct);
            return docs is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(docs));
        }

        [HttpGet("{id:int}/children")]
        public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> GetChildren(int id, CancellationToken ct)
        {
            var docs = await _service.GetChildrenAsync(id, ct);
            return docs is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(docs));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<DocumentResponse>> Create(
            [FromBody] CreateDocumentRequest request,
            CancellationToken ct)
        {
            var doc = await _service.CreateAsync(
                request.DocNumber,
                request.DocTypeId,
                request.DocDate,
                request.CurrencyId,
                request.TotalAmount,
                request.SupplierId,
                request.BuyerId,
                request.VesselId,
                request.PortId,
                request.ParentDocumentId,
                request.Reference,
                request.FilePath,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = doc.Id }, ApiResponse<DocumentResponse>.Ok(doc));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentResponse>>>> CreateRange(
            [FromBody] IEnumerable<CreateDocumentRequest> requests,
            CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateDocumentRequest
            {
                DocNumber = r.DocNumber,
                DocTypeId = r.DocTypeId,
                DocDate = r.DocDate,
                CurrencyId = r.CurrencyId,
                TotalAmount = r.TotalAmount,
                SupplierId = r.SupplierId,
                BuyerId = r.BuyerId,
                VesselId = r.VesselId,
                PortId = r.PortId,
                ParentDocumentId = r.ParentDocumentId,
                Reference = r.Reference,
                FilePath = r.FilePath
            });

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(result));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(id,
                request.DocTypeId,
                request.DocDate,
                request.CurrencyId,
                request.TotalAmount,
                request.SupplierId,
                request.BuyerId,
                request.VesselId,
                request.PortId,
                request.Reference,
                request.FilePath,
                ct);

            return NoContent();
        }

        [HttpPatch("{id:int}/link-parent")]
        public async Task<IActionResult> LinkToParent(int id, [FromBody] LinkParentRequest request, CancellationToken ct)
        {
            await _service.LinkToParentAsync(id, request.ParentDocumentId, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/unlink-parent")]
        public async Task<IActionResult> UnlinkFromParent(int id, CancellationToken ct)
        {
            await _service.UnlinkFromParentAsync(id, ct);
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

        // ─────────────────────────────────────────────
        // Items
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/items")]
        public async Task<ActionResult<DocumentItemResponse>> AddItem(int id, [FromBody] AddDocumentItemRequest request, CancellationToken ct)
        {
            var item = await _service.AddItemAsync(id, request.ProductName, request.Quantity, request.UnitPrice, request.Unit, ct);
            return Ok(item);
        }

        [HttpPost("{documentId:int}/items/batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentItemResponse>>>> AddItemsRange(
        int documentId,
        [FromBody] IEnumerable<AddDocumentItemRequest> requests,
        CancellationToken ct)
        {
            var commands = requests.Select(r => new AddDocumentItemRequest
            {
                ProductName = r.ProductName,
                Quantity = r.Quantity,
                UnitPrice = r.UnitPrice,
                Unit = r.Unit
            });

            var result = await _service.AddItemsRangeAsync(documentId, commands, ct);
            return Ok(ApiResponse<IReadOnlyList<DocumentItemResponse>>.Ok(result));
        }

        [HttpPut("{id:int}/items/{itemId:int}")]
        public async Task<IActionResult> UpdateItem(int id, int itemId, [FromBody] UpdateDocumentItemRequest request, CancellationToken ct)
        {
            await _service.UpdateItemAsync(id, itemId, request.ProductName, request.Quantity, request.UnitPrice, request.Unit, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}/items/{itemId:int}")]
        public async Task<IActionResult> RemoveItem(int id, int itemId, CancellationToken ct)
        {
            await _service.RemoveItemAsync(id, itemId, ct);
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // Payments
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/payments")]
        public async Task<ActionResult<PaymentResponse>> AddPayment(int id, [FromBody] AddPaymentRequest request, CancellationToken ct)
        {
            var payment = await _service.AddPaymentAsync(id, request.SwiftTransferId, request.PaidAmount, request.PaymentDate, ct);
            return Ok(payment);
        }

        // ─────────────────────────────────────────────
        // Email
        // ─────────────────────────────────────────────

        [HttpPost("{id:int}/email")]
        public async Task<IActionResult> LogEmail(int id, [FromBody] LogEmailRequest request, CancellationToken ct)
        {
            await _service.LogEmailAsync(id, request.Subject, request.Body, request.Participants, request.Direction, ct);
            return NoContent();
        } 
    }
}
