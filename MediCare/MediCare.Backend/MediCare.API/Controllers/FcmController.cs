using System.Security.Claims;
using MediCare.Application.Modules.FCM;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FcmController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FcmController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/fcm/save-token
        [HttpPost("save-token")]
        public async Task<IActionResult> SaveToken([FromBody] FcmTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest("Token is required.");


            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            // Pošalji command handler-u
            var command = new SaveFcmTokenCommand(userId,dto.Token);
            var result = await _mediator.Send(command);

            if (result)
                return Ok(new { message = "Token saved successfully." });

            return StatusCode(500, "Failed to save token.");
        }

        // GET: api/fcm/tokens (za test)
        [HttpGet("tokens")]
        public IActionResult GetTokens()
        {
            // Dohvati sve tokene iz handler-a
            var tokens = SaveFcmTokenHandler.GetAllTokens();
            return Ok(tokens);
        }
    }
}
