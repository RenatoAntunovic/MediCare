namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Create;

public class CreateTreatmentCategoryCommand : IRequest<int>
{
    public required string Name { get; set; }
}