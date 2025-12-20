using MediCare.Application.Modules.Medicine.Medicine.Commands.Create;

public class CreateMedicineCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateMedicineCommand, int>
{
    public async Task<int> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name?.Trim();

        //if (string.IsNullOrWhiteSpace(normalized))
        //    throw new ValidationException("Name is required."); ovo sam zakomentarisao zato sto se sve vec testira u validatoru

        // Check if a category with the same name already exists.
        bool exists = await context.Medicine
            .AnyAsync(x => x.Name == normalized, cancellationToken);

        if (exists)
        {
            throw new MediCareConflictException("Name already exists.");
        }

        var medicine = new Medicine
        {
            Name = request.Name!.Trim(),
            Description = request.Description,
            MedicineCategoryId = request.MedicineCategoryId,
            Weight = request.Weight,
            Price = request.Price,
            isEnabled = request.isEnabled
        };

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images"); // folder za slike
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ImageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.ImageFile.CopyToAsync(fileStream, cancellationToken);
            }

            // Spremi relativnu putanju u bazu
            medicine.ImagePath = "images/" + uniqueFileName;
        }

        context.Medicine.Add(medicine);
        await context.SaveChangesAsync(cancellationToken);

        return medicine.Id;
    }
}