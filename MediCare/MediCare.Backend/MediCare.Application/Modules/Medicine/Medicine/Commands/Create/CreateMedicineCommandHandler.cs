using MediCare.Application.Modules.Medicine.Medicine.Commands.Create;
using MediCare.Application.Modules.MedicineSearch;

public class CreateMedicineCommandHandler(IAppDbContext context, IMedicineSearchIndex searchIndex)
    : IRequestHandler<CreateMedicineCommand, int>
{
    public async Task<int> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name.Trim();

        bool exists = await context.Medicine
            .AnyAsync(x => x.Name == normalized, cancellationToken);

        if (exists)
            throw new MediCareConflictException("Lijek s tim nazivom već postoji.");

        var medicine = new Medicine
        {
            Name = normalized,
            Description = request.Description,
            MedicineCategoryId = request.MedicineCategoryId,
            Weight = request.Weight,
            Price = request.Price,
            isEnabled = request.isEnabled
        };

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Path.GetFileName strips any path segments from the file name sent by the client
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.ImageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.ImageFile.CopyToAsync(fileStream, cancellationToken);
            }

            medicine.ImagePath = "images/" + uniqueFileName;
        }

        context.Medicine.Add(medicine);
        await context.SaveChangesAsync(cancellationToken);

        // Sync the search index (a no-op if ES is disabled)
        await searchIndex.SyncMedicineAsync(context, medicine.Id, cancellationToken);

        return medicine.Id;
    }
}
