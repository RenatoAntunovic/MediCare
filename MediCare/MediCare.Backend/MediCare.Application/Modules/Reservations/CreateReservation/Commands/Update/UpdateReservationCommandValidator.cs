using FluentValidation;
using MediCare.Application.Modules.Reservations.Common;

namespace MediCare.Application.Modules.Reservations.Commands.Update
{
    public class UpdateReservationCommandValidator : AbstractValidator<UpdateReservationCommand>
    {
        public UpdateReservationCommandValidator()
        {
            RuleFor(x => x.ReservationId)
                .GreaterThan(0).WithMessage("ID rezervacije je obavezan.");

            RuleFor(x => x.TreatmentId)
                .GreaterThan(0).WithMessage("Tretman je obavezan.");

            RuleFor(x => x.ReservationDate)
                .Must(ReservationRules.IsBookableDate)
                .WithMessage("Termin se može rezervisati najranije za sutra, i to samo radnim danom.");

            RuleFor(x => x.ReservationTime)
                .Must(ReservationRules.IsValidSlot)
                .WithMessage("Termin mora biti između 08:00 i 17:00, na puni sat ili pola sata.");
        }
    }
}
