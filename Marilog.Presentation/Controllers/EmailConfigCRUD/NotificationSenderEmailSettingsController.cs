using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.EmailConfigCRUD
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationSenderEmailSettingsController : ControllerBase
    {
        private readonly INotificationSenderEmailSettingsStore _store;

        public NotificationSenderEmailSettingsController(INotificationSenderEmailSettingsStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<ActionResult<NotificationSenderEmailSettingsOptions>> Get(CancellationToken cancellationToken)
        {
            var options = await _store.GetAsync(cancellationToken);

            return Ok(options);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] NotificationSenderEmailSettingsOptions options, CancellationToken cancellationToken)
        {
            await _store.UpdateAsync(options, cancellationToken);

            return NoContent();
        }
    }
}
