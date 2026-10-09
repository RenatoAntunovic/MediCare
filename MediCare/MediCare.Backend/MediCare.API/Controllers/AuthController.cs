using System.Security.Claims;
using MediatR;
using MediCare.Application.Modules.Auth.Commands.Login;
using MediCare.Application.Modules.Auth.Commands.Logout;
using MediCare.Application.Modules.Auth.Commands.Refresh;
using MediCare.Application.Modules.Auth.Commands.Register;
using MediCare.Application.Modules.Auth.Queries.GetUserById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender; // MediatR sender

    // Constructor - dependency injection
    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginCommandDto>> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterCommandDto>> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        int id = await _sender.Send(command, ct);

        // Return 201 Created with the location of the new user
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginCommandDto>> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        await _sender.Send(command, ct);
        return NoContent();
    }

    // GetById endpoint used by CreatedAtAction
    // Users can only see their own profile; admins can see anyone
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<RegisterCommandDto>> GetById(int id, CancellationToken ct)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (currentUserId != id && !User.IsInRole("Admin"))
            return Forbid();

        var user = await _sender.Send(new GetUserByIdQuery(id), ct);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
