using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Update;

public sealed class UpdateTreatmentsCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public string ServiceName { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public IFormFile? ImageFile { get; set; }
    public int TreatmentCategoryId { get; set; }
    public bool isEnabled { get; set; }

}
