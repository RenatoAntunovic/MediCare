namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Disable;

public sealed class DisableTreatmentCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
