using MediatR;
using MediCare.Application.Modules.Reservations.Commands.Status.ChangeStatus;
using MediCare.Application.Modules.Reservations.Commands.Update;
using MediCare.Application.Modules.Reservations.Commands.Update;
using MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create;
using MediCare.Application.Modules.Reservations.CreateReservation.Queries.GetReservations;
using MediCare.Application.Modules.Reservations.Queries.GetById;
using MediCare.Application.Modules.Reservations.Queries.GetById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediCare.Application.Modules.Reservations.Commands.Status.ChangeStatus;

namespace MediCare.API.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { error = "User ID nije pronađen u tokenu." });
                }

                command.UserId = userId;
                int reservationId = await _mediator.Send(command);

                return Ok(new { reservationId, message = "Rezervacija uspješno kreirana." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserReservations()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { error = "User ID nije pronađen u tokenu." });
                }

                var query = new GetUserReservationsQuery { UserId = userId };
                var reservations = await _mediator.Send(query);

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { error = "User ID nije pronađen u tokenu." });
                }

                var query = new GetReservationByIdQuery
                {
                    ReservationId = id,
                    UserId = userId
                };

                var reservation = await _mediator.Send(query);

                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationCommand command)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { error = "User ID nije pronađen u tokenu." });
                }

                command.ReservationId = id;
                command.UserId = userId;

                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

     

        [HttpPut("{id:int}/change-status")]
        [Authorize(Roles = "Admin")]
        public async Task ChangeStatus(int id, [FromBody] ChangeReservationStatusCommand command, CancellationToken ct)
        {
            command.Id = id;
            await _mediator.Send(command, ct);
        }


}
}
