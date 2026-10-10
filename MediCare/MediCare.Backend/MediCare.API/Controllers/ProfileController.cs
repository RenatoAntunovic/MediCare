using MediCare.Application.Modules.Profile;

namespace MediCare.API.Controllers;

/// <summary>The logged-in user's own profile (Settings page).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(ISender sender) : ControllerBase
{
    // GET api/profile
    [HttpGet]
    public async Task<MyProfileDto> Get(CancellationToken ct)
        => await sender.Send(new GetMyProfileQuery(), ct);

    // PUT api/profile
    [HttpPut]
    public async Task<MyProfileDto> Update([FromBody] UpdateMyProfileCommand command, CancellationToken ct)
        => await sender.Send(command, ct);
}