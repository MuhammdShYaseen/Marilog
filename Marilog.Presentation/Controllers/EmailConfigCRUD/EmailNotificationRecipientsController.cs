using Marilog.Contracts.DTOs.Requests.EmailNotifiConfig;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.EmailConfigCRUD
{
    [ApiController]
    [Route("api/notification-recipients")]
    public sealed class EmailNotificationRecipientsController : ControllerBase
    {
        private readonly INotificationRecipientStore _recipientStore;

        public EmailNotificationRecipientsController(INotificationRecipientStore recipientStore)
        {
            _recipientStore = recipientStore;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<string>>> GetAll(CancellationToken cancellationToken)
        {
            var recipients = await _recipientStore.GetAllAsync(cancellationToken);

            return Ok(recipients);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] string email, CancellationToken cancellationToken)
        {
            await _recipientStore.AddAsync(email, cancellationToken);

            return NoContent();
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> AddRange([FromBody] IReadOnlyList<string> emails, CancellationToken cancellationToken)
        {
            await _recipientStore.AddRangeAsync(emails, cancellationToken);

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateNotificationRecipientRequest request, CancellationToken cancellationToken)
        {
            await _recipientStore.UpdateAsync(request.CurrentEmail, request.NewEmail, cancellationToken);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Remove([FromQuery] string email, CancellationToken cancellationToken)
        {
            await _recipientStore.RemoveAsync(email, cancellationToken);

            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> RemoveRange([FromBody] IReadOnlyList<string> emails, CancellationToken cancellationToken)
        {
            await _recipientStore.RemoveRangeAsync(emails, cancellationToken);

            return NoContent();
        }
    }
}
