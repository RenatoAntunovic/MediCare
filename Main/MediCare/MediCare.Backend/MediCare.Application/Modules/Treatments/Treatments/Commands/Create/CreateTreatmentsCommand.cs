namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Create;

public class CreateTreatmentsCommand : IRequest<int>
{
    public string ServiceName { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string ImagePath { get; set; }
    public int TreatmentCategoryId { get; set; }
    public bool isEnabled { get; set; }

}