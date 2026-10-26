using MediCare.Application.Modules.Reservations.Common;

namespace MediCare.Application.Modules.Reservations.CreateReservation.Commands.Create;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.TreatmentId)
            .GreaterThan(0).WithMessage("Tretman je obavezan.");

        RuleFor(x => x.ReservationDate)
            .Must(ReservationRules.IsBookableDate)
            .WithMessage("Termin se može rezervisati najranije za sutra, i to samo radnim danom.");

        RuleFor(x => x.ReservationTime)
            .Must(ReservationRules.IsValidSlot)
            .WithMessage("Termin mora biti između 08:00 i 17:00, na puni sat ili pola sata.");

        RuleFor(x => x.Notes)
            .MaximumLength(ReservationRules.NotesMaxLength)
            .WithMessage($"Opis može imati najviše {ReservationRules.NotesMaxLength} znakova.");
    }
}
