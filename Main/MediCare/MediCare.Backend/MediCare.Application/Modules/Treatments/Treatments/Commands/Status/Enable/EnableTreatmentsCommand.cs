namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Enable;

public sealed class EnableTreatmentsCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
