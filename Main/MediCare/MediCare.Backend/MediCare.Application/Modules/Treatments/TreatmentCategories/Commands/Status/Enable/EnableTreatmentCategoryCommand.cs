namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Enable;

public sealed class EnableTreatmentCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
