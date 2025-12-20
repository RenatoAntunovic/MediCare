using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Update;

public sealed class UpdateMedicineCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; } 
    public string Description { get; set; }
    public IFormFile? ImageFile { get; set; } 
    public int MedicineCategoryId { get; set; }
    public int Weight { get; set; } 
    public decimal Price { get; set; }
    public bool isEnabled { get; set; }
    
}
