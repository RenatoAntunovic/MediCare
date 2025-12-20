namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Disable;

public sealed class DisableTreatmentsCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
