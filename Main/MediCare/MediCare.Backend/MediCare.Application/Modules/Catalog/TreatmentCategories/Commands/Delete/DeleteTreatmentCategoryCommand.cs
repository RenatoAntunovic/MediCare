namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Delete;

public class DeleteTreatmentCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
