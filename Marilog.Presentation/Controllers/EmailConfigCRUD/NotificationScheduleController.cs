using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.EmailConfigCRUD
{
    [ApiController]
    [Route("api/email-notifications/schedule")]
    public sealed class NotificationScheduleController : ControllerBase
    {
        private readonly INotificationSchedule _notificationSchedule;

        public NotificationScheduleController(INotificationSchedule notificationSchedule)
        {
            _notificationSchedule = notificationSchedule;
        }

        [HttpGet]
        [ProducesResponseType(typeof(NotificationScheduleOptions), StatusCodes.Status200OK)]
        public async Task<ActionResult<NotificationScheduleOptions>> GetAsync(CancellationToken cancellationToken)
        {
            var schedule = await _notificationSchedule.GetAsync(cancellationToken);

            return Ok(schedule);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveAsync([FromBody] NotificationScheduleOptions options, CancellationToken cancellationToken)
        {
            await _notificationSchedule.SaveAsync(options, cancellationToken);
            return NoContent();
        }

        [HttpGet("next-execution")]
        [ProducesResponseType(typeof(DateTimeOffset), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<DateTimeOffset>> GetNextExecutionAsync(CancellationToken cancellationToken)
        {
            var nextExecution = await _notificationSchedule.GetNextExecutionAsync(DateTimeOffset.UtcNow, cancellationToken);

            if (nextExecution is null)
                return NoContent();

            return Ok(nextExecution.Value);
        }
    }
}
