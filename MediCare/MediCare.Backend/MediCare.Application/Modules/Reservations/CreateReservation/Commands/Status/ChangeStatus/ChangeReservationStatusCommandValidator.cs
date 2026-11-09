namespace MediCare.Application.Modules.Reservations.Commands.Status.ChangeStatus;

/// <summary>Both ids must be positive before the handler looks the reservation and the status up.</summary>
public sealed class ChangeReservationStatusCommandValidator : AbstractValidator<ChangeReservationStatusCommand>
{
    public ChangeReservationStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NewStatusId).GreaterThan(0);
    }
}
