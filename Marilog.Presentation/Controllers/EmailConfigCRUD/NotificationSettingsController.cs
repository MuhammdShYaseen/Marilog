using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Contracts.Options;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.EmailConfigCRUD
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class NotificationSettingsController : ControllerBase
    {
        private readonly INotificationSettingsStore _store;

        public NotificationSettingsController(
            INotificationSettingsStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<ActionResult<NotificationSettingsOptions>> Get(
            CancellationToken cancellationToken)
        {
            var options = await _store.GetAsync(cancellationToken);

            return Ok(options);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] NotificationSettingsOptions options,
            CancellationToken cancellationToken)
        {
            await _store.UpdateAsync(options, cancellationToken);

            return NoContent();
        }
    }
}