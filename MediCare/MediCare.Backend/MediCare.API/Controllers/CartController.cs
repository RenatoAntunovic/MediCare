using System.Security.Claims;
using MediCare.Application.Modules.Cart.Command.AddToCart;
using MediCare.Application.Modules.Cart.Command.AddToCartFromFavourites;
using MediCare.Application.Modules.Cart.Command.AddToCartFromForLater;
using MediCare.Application.Modules.Cart.Command.Checkout;
using MediCare.Application.Modules.Cart.Command.Delete;
using MediCare.Application.Modules.Cart.Queries;
using MediCare.Application.Abstractions;
using MediCare.Application.Modules.Cart.Command.SetQuantity;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAppDbContext _context;

        public CartController(IMediator mediator, IAppDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        // GET /api/cart
        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            var result = await _mediator.Send(new GetUserCartQuery());
            return Ok(result);
        }

        // POST /api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command)
        {
            int cartItemId = await _mediator.Send(command);
            return Ok(new { cartItemId });
        }

        // DELETE /api/cart/items/{id}
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            await _mediator.Send(new DeleteCartItemCommand { Id = id });
            return NoContent();
        }

        // PUT /api/cart/items/{id} – set the quantity of one cart item
        [HttpPut("items/{id}")]
        public async Task<ActionResult<SetCartItemQuantityResultDto>> SetQuantity(
            int id, [FromBody] SetCartItemQuantityCommand command, CancellationToken ct)
        {
            command.CartItemId = id;
            return Ok(await _mediator.Send(command, ct));
        }

        [Authorize]
        [HttpPost("add-from-favourites")]
        public async Task<IActionResult> AddFromFavourites([FromBody] AddToCartFromFavouritesDto dto)
        {
            var userIdClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(); // or BadRequest

            var command = new AddToCartFromFavouritesCommand(userId, dto.FavouriteId, dto.Quantity);
            var result = await _mediator.Send(command);

            if (!result)
                return BadRequest("Ovaj lijek nije u Favorites.");

            return Ok(new { message = "Dodano u korpu." });

        }

        [Authorize]
        [HttpPost("add-from-for-later")]
        public async Task<IActionResult> AddFromForLater([FromBody] AddToCartFromForLaterDto dto)
        {
            var userIdClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(); // or BadRequest

            var command = new AddToCartFromForLaterCommand(userId, dto.ForLaterId, dto.Quantity);
            var result = await _mediator.Send(command);

            if (!result)
                return BadRequest("Ovaj lijek nije u za kasnije.");

            return Ok(new { message = "Dodano u korpu." });

        }

        [HttpPost("checkout")]
        [EnableRateLimiting("orders")]
        public async Task<IActionResult> Checkout(CancellationToken ct)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var response = await _mediator.Send(
                new CheckoutOrderCommand { UserId = userId },
                ct
            );

            return Ok(response);
        }
    }
}
