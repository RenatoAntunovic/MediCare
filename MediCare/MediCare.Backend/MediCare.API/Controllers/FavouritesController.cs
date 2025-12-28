using MediCare.Application.Modules.Favourites.Command.Create;
using MediCare.Application.Modules.Favourites.Command.Delete;
using MediCare.Application.Modules.Favourites.Queries;


namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavouritesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FavouritesController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        public async Task<IActionResult> GetFavourites()
        {
            var result = await _mediator.Send(new GetUserFavouritesQuery());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavourite([FromBody] AddToFavouritesCommand command)
        {
            int favouriteId = await _mediator.Send(command);
            return Ok(new { favouriteId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavourites(int id)
        {
            await _mediator.Send(new DeleteFavouritesCommand { Id = id });
            return NoContent();
        }
    }
}
