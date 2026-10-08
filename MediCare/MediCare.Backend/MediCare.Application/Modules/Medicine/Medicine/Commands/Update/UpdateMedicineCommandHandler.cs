using MediCare.Application.Modules.MedicineSearch;

namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Update;

public sealed class UpdateMedicineCommandHandler(IAppDbContext ctx, IMedicineSearchIndex searchIndex)
    : IRequestHandler<UpdateMedicineCommand, Unit>
{
    public async Task<Unit> Handle(UpdateMedicineCommand request, CancellationToken ct)
    {
        var entity = await ctx.Medicine
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Lijek (ID={request.Id}) nije pronađen.");

        var normalizedName = request.Name.Trim();

        var exists = await ctx.Medicine
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == normalizedName.ToLower(), ct);

        if (exists)
            throw new MediCareConflictException("Lijek s tim nazivom već postoji.");

        var categoryExists = await ctx.MedicineCategories
            .AnyAsync(c => c.Id == request.MedicineCategoryId, ct);

        if (!categoryExists)
            throw new MediCareNotFoundException("Odabrana kategorija ne postoji.");

        entity.Name = normalizedName;
        entity.Description = request.Description ?? string.Empty;
        entity.Price = request.Price;
        entity.Weight = request.Weight;
        entity.MedicineCategoryId = request.MedicineCategoryId;
        entity.isEnabled = request.isEnabled;

        // The image is optional (inline edit does not send it)
        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrWhiteSpace(entity.ImagePath))
            {
                // TrimStart: seed paths start with "/", so Path.Combine would return an absolute path
                var oldPath = Path.Combine("wwwroot", entity.ImagePath.TrimStart('/', '\\'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.ImageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await request.ImageFile.CopyToAsync(stream, ct);

            entity.ImagePath = $"images/{uniqueFileName}";
        }

        await ctx.SaveChangesAsync(ct);

        await searchIndex.SyncMedicineAsync(ctx, entity.Id, ct);

        return Unit.Value;
    }
}
