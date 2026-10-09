using MediCare.Application.Abstractions;
using MediCare.Application.Modules.Notifications;

namespace MediCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(
    ISender sender,
    IPushNotificationService push,
    IAppCurrentUser currentUser) : ControllerBase
{
    // POST api/notifications/token – the browser registers its FCM token for the current user
    [HttpPost("token")]
    public async Task<IActionResult> SaveToken([FromBody] SaveFcmTokenCommand command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return NoContent();
    }

    // POST api/notifications/test – sends a test notification to yourself
    [HttpPost("test")]
    [EnableRateLimiting("notifications")]
    public async Task<IActionResult> SendTest(CancellationToken ct)
    {
        await push.SendToUserAsync(currentUser.UserId!.Value,
            "MediCare test", "Notifikacije rade! 🎉", ct);
        return NoContent();
    }
}