using MediCare.Application.Modules.Assistant.Commands.Chat;

namespace MediCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssistantController(ISender sender) : ControllerBase
{
    // POST /api/Assistant/chat
    // Patients and admins get different tools (decided by role in the handler).
    [HttpPost("chat")]
    [EnableRateLimiting("assistant")]
    public async Task<AssistantChatResponseDto> Chat(AssistantChatCommand command, CancellationToken ct)
    {
        return await sender.Send(command, ct);
    }
}
