namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Delete;

public class DeleteTreatmentsCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
