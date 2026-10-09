using System.Security.Claims;
using MediCare.Application.Modules.Reservations.Commands.Status.ChangeStatus;
using MediCare.Application.Modules.Reservations.Commands.Update;
using MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create;
using MediCare.Application.Modules.Reservations.CreateReservation.Queries.GetReservations;
using MediCare.Application.Modules.Reservations.Queries.GetById;
using MediCare.Application.Modules.Reservations.Queries.TakenSlots;

namespace MediCare.API.Controllers
{
    /// <summary>
    /// Errors (404/409/400) are handled by the global MarketExceptionHandler, so there is no try/catch here.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/reservations
        [HttpPost]
        [EnableRateLimiting("orders")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command, CancellationToken ct)
        {
            command.UserId = GetUserId();
            var reservationId = await _mediator.Send(command, ct);

            return Ok(new { reservationId, message = "Rezervacija je uspješno kreirana." });
        }

        // GET /api/reservations  → reservations of the signed-in user
        [HttpGet]
        public async Task<IActionResult> GetUserReservations(CancellationToken ct)
        {
            var reservations = await _mediator.Send(new GetUserReservationsQuery { UserId = GetUserId() }, ct);
            return Ok(reservations);
        }

        // GET /api/reservations/availability?treatmentId=1&date=2026-10-20
        [HttpGet("availability")]
        public async Task<ActionResult<List<string>>> GetTakenSlots(
            [FromQuery] int treatmentId,
            [FromQuery] DateTime date,
            CancellationToken ct)
        {
            var taken = await _mediator.Send(new GetTakenSlotsQuery { TreatmentId = treatmentId, Date = date }, ct);
            return Ok(taken);
        }

        // GET /api/reservations/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReservationById(int id, CancellationToken ct)
        {
            var reservation = await _mediator.Send(new GetReservationByIdQuery
            {
                ReservationId = id,
                UserId = GetUserId()
            }, ct);

            return Ok(reservation);
        }

        // PUT /api/reservations/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationCommand command, CancellationToken ct)
        {
            command.ReservationId = id;
            command.UserId = GetUserId();

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        // PUT /api/reservations/5/change-status
        [HttpPut("{id:int}/change-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeReservationStatusCommand command, CancellationToken ct)
        {
            command.Id = id;
            await _mediator.Send(command, ct);
            return NoContent();
        }

        private int GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (!int.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("User ID nije pronađen u tokenu.");

            return userId;
        }
    }
}
