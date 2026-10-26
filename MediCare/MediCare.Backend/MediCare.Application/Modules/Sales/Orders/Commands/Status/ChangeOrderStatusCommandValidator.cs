namespace MediCare.Application.Modules.Sales.Orders.Commands.Status;

public sealed class ChangeOrderStatusCommandValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NewStatusId).GreaterThan(0).WithMessage("Status must be selected.");
    }
}