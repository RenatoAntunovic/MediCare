namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Update;

public sealed class UpdateTreatmentCategoryCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public required string Name { get; set; }
}
