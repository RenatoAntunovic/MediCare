using MediCare.Application.Modules.MedicineSearch;
using Microsoft.Extensions.Logging;

namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Update;

public sealed class UpdateMedicineCommandHandler(
    IAppDbContext ctx,
    IMedicineSearchIndex searchIndex,
    IImageStorage storage,
    ILogger<UpdateMedicineCommandHandler> logger)
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

        var category = await ctx.MedicineCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.MedicineCategoryId, ct);

        if (category is null)
            throw new MediCareNotFoundException("Odabrana kategorija ne postoji.");

        // Same rule as disabling a category: an active medicine cannot belong to a disabled category
        if (request.isEnabled && !category.IsEnabled)
            throw new MediCareBusinessRuleException("medicine.category.disabled",
                "Aktivan lijek ne može pripadati neaktivnoj kategoriji.");

        entity.Name = normalizedName;
        entity.Description = request.Description ?? string.Empty;
        entity.Price = request.Price;
        entity.Weight = request.Weight;
        entity.MedicineCategoryId = request.MedicineCategoryId;
        entity.isEnabled = request.isEnabled;

        var oldImagePath = entity.ImagePath;
        string? newImagePath = null;

        // The image is optional (inline edit does not send it)
        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            // 1) save the NEW file under its own unique name first.
            //    If this fails, the old file and the database are untouched.
            newImagePath = await storage.SaveAsync(request.ImageFile, ct);
            entity.ImagePath = newImagePath;
        }

        // 2) write the new reference to the database
        try
        {
            await ctx.SaveChangesAsync(ct);
        }
        catch
        {
            // the medicine keeps its old image, so the new file is not needed
            if (newImagePath is not null)
                storage.Delete(newImagePath);
            throw;
        }

        // 3) only now remove the old file (if nobody else uses it); a failure here must not fail the request
        if (newImagePath is not null && !string.IsNullOrWhiteSpace(oldImagePath))
            await TryDeleteOldImageAsync(oldImagePath, entity.Id, ct);

        await searchIndex.SyncMedicineAsync(ctx, entity.Id, ct);

        return Unit.Value;
    }

    private async Task TryDeleteOldImageAsync(string oldImagePath, int medicineId, CancellationToken ct)
    {
        try
        {
            // The same file can be shared (the seed uses one image for several items),
            // so it is deleted only if no other medicine or treatment still points to it.
            var relative = oldImagePath.TrimStart('/', '\\');
            var withSlash = "/" + relative;

            var usedElsewhere =
                await ctx.Medicine.AnyAsync(m => m.Id != medicineId && (m.ImagePath == relative || m.ImagePath == withSlash), ct) ||
                await ctx.Treatments.AnyAsync(t => t.ImagePath == relative || t.ImagePath == withSlash, ct);

            if (!usedElsewhere)
                storage.Delete(relative);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Old medicine image {Path} could not be removed.", oldImagePath);
        }
    }
}
