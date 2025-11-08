using System.ComponentModel;

namespace MediCare.Application.Modules.Catalog.Medicine.Commands.Update;

public sealed class UpdateMedicineCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; } 
    public string Description { get; set; }
    public string ImagePath { get; set; } 
    public int MedicineCategoryId { get; set; }
    public int Weight { get; set; } 
    public decimal Price { get; set; }
    
}
