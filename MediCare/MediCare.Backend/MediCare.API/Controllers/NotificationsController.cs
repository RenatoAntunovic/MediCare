using MediCare.Application.Modules.Notifications.Commands.RemoveFcmToken;
using MediCare.Application.Modules.Notifications.Commands.SaveFcmToken;
using MediCare.Application.Modules.Notifications.Commands.SendTestNotification;

namespace MediCare.API.Controllers;

/// <summary>Push notifications of the logged-in user (FCM token registration and a test message).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(ISender sender) : ControllerBase
{
    // POST api/notifications/token – the browser registers its FCM token for the current user
    [HttpPost("token")]
    public async Task<IActionResult> SaveToken([FromBody] SaveFcmTokenCommand command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return NoContent();
    }

    // DELETE api/notifications/token – turn push notifications off for the current user
    [HttpDelete("token")]
    public async Task<IActionResult> RemoveToken(CancellationToken ct)
    {
        await sender.Send(new RemoveFcmTokenCommand(), ct);
        return NoContent();
    }

    // POST api/notifications/test – sends a test notification to yourself
    [HttpPost("test")]
    [EnableRateLimiting("notifications")]
    public async Task<IActionResult> SendTest(CancellationToken ct)
    {
        await sender.Send(new SendTestNotificationCommand(), ct);
        return NoContent();
    }
}
