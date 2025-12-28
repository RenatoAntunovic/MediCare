using MediCare.Domain.Entities.HospitalRecords;

namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Update;

public sealed class UpdateMedicineCommandHandler(IAppDbContext ctx)
    : IRequestHandler<UpdateMedicineCommand, Unit>
{
    public async Task<Unit> Handle(UpdateMedicineCommand request, CancellationToken ct)
    {
        var entity = await ctx.Medicine
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Medicine (ID={request.Id}) not found.");

        // Duplicate name check
        var exists = await ctx.Medicine
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists)
            throw new MediCareConflictException("Name already exists.");

        // Basic fields
        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.Price = request.Price;
        entity.Weight = request.Weight;
        entity.MedicineCategoryId = request.MedicineCategoryId;
        entity.isEnabled = request.isEnabled;

        // ✅ IMAGE UPDATE (OPCIONALNO)
        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // (opciono) obriši staru sliku
            if (!string.IsNullOrWhiteSpace(entity.ImagePath))
            {
                var oldPath = Path.Combine("wwwroot", entity.ImagePath);
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{request.ImageFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await request.ImageFile.CopyToAsync(stream, ct);

            // ⬅️ JEDINO MJESTO gdje se postavlja ImagePath
            entity.ImagePath = $"images/{uniqueFileName}";
        }

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
