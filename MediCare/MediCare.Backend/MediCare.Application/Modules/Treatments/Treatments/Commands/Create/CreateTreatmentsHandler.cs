using MediCare.Application.Modules.Catalog.Treatments.Commands.Create;
using MediCare.Domain.Entities.HospitalRecords;

public class CreateTreatmentsCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateTreatmentsCommand, int>
{
    public async Task<int> Handle(CreateTreatmentsCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.ServiceName?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Name is required.");

        // Check if a category with the same name already exists.
        bool exists = await context.Treatments
            .AnyAsync(x => x.ServiceName == normalized, cancellationToken);

        if (exists)
        {
            throw new MediCareConflictException("Name already exists.");
        }

        var treatment = new Treatments
        {
            ServiceName = request.ServiceName!.Trim(),
            Description = request.Description,
            TreatmentCategoryId = request.TreatmentCategoryId,
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
            treatment.ImagePath = "images/" + uniqueFileName;
        }

        context.Treatments.Add(treatment);
        await context.SaveChangesAsync(cancellationToken);

        return treatment.Id;
    }
}