using MediCare.Application.Modules.ForLater.Command.Create;
using MediCare.Application.Modules.ForLater.Command.Delete;
using MediCare.Application.Modules.ForLater.Queries;


namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForLaterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForLaterController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        public async Task<IActionResult> GetForLater()
        {
            var result = await _mediator.Send(new GetForLaterQuery());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddToForLater([FromBody] AddToForLaterCommand command)
        {
            int forLaterId = await _mediator.Send(command);
            return Ok(new { forLaterId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForLater(int id)
        {
            await _mediator.Send(new DeleteForLaterCommand { Id = id });
            return NoContent();
        }
    }
}
