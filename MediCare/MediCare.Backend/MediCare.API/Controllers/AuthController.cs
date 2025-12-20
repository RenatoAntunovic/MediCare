using MediCare.Application.Modules.Auth.Commands.Login;
using MediCare.Application.Modules.Auth.Commands.Logout;
using MediCare.Application.Modules.Auth.Commands.Refresh;
using MediCare.Application.Modules.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediCare.Application.Modules.Auth.Queries.GetUserById;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender; // MediatR sender

    // Konstruktor - dependency injection
    public AuthController(ISender sender)
    {
        _sender = sender;
    }

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

        // Vraćamo 201 Created sa lokacijom novog korisnika
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

    // Primjer GetById endpoint-a za CreatedAtAction
    [HttpGet("{id}")]
    public async Task<ActionResult<RegisterCommandDto>> GetById(int id, CancellationToken ct)
    {
        var user = await _sender.Send(new GetUserByIdQuery(id), ct); // moraš imati query za ovo
        if (user == null) return NotFound();
        return Ok(user);
    }
}
