using FluentValidation;

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
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Datum mora biti u budućnosti.");

            RuleFor(x => x.ReservationTime)
                .NotEmpty().WithMessage("Vrijeme je obavezno.");
        }
    }
}
