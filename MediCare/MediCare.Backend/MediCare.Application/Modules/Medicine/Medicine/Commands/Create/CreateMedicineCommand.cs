using Microsoft.AspNetCore.Http;

namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Create;

public class CreateMedicineCommand : IRequest<int>
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required string Description { get; set; }
    public required int MedicineCategoryId { get; set; }
    public required IFormFile ImageFile { get; set; }
    public required int Weight { get; set; }
    public required bool isEnabled { get; set; }

}