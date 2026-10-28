using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/documents/{documentId:int}")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentsController(IPaymentService service)
        {
            _service = service;
        }
        [RequirePermission(EntityType.Payment, PermissionAction.Create)]
        [HttpPost("payments")]
        public async Task<ActionResult<PaymentResponse>> AddPayment(int documentId, [FromBody] AddPaymentRequest request, CancellationToken ct)
        {
            var payment = await _service.AddPaymentAsync(documentId, request, ct);
            return Ok(payment);
        }

        [RequirePermission(EntityType.Payment, PermissionAction.Update)]
        [HttpPut("payments/{paymentId:int}")]
        public async Task<ActionResult<PaymentResponse>> UpdatePayment(int documentId, int paymentId, [FromBody] UpdatePaymentRequest request, CancellationToken ct)
        {
            var payment = await _service.UpdatePaymentAsync(documentId, paymentId, request, ct);
            return Ok(payment);
        }

        [RequirePermission(EntityType.Payment, PermissionAction.Delete)]
        [HttpDelete("payment/{paymentId:int}")]
        public async Task<IActionResult> RemovePayment(int documentId, int paymentId, CancellationToken ct)
        {
            await _service.RemovePaymentAsync(documentId, paymentId, ct);
            return NoContent();
        }
    }
}
