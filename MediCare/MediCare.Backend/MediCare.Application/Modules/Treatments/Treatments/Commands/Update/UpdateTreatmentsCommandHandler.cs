using Microsoft.Extensions.Logging;

namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Update;

public sealed class UpdateTreatmentsCommandHandler(
    IAppDbContext ctx,
    IImageStorage storage,
    ILogger<UpdateTreatmentsCommandHandler> logger)
            : IRequestHandler<UpdateTreatmentsCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTreatmentsCommand request, CancellationToken ct)
    {
        var entity = await ctx.Treatments
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Treatment (ID={request.Id}) not found.");

        var normalizedName = request.ServiceName.Trim();

        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await ctx.Treatments
            .AnyAsync(x => x.Id != request.Id && x.ServiceName.ToLower() == normalizedName.ToLower(), ct);

        if (exists)
            throw new MediCareConflictException("Tretman s tim nazivom već postoji.");

        var categoryExists = await ctx.TreatmentCategories
            .AnyAsync(c => c.Id == request.TreatmentCategoryId, ct);

        if (!categoryExists)
            throw new MediCareNotFoundException("Odabrana kategorija ne postoji.");

        // isEnabled is NOT changed here – status is changed only via the Enable/Disable endpoints
        entity.ServiceName = normalizedName;
        entity.Price = request.Price;
        entity.TreatmentCategoryId = request.TreatmentCategoryId;
        entity.Description = request.Description;

        var oldImagePath = entity.ImagePath;
        string? newImagePath = null;

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            // 1) save the NEW file first. If this fails, the old file and the database are untouched.
            newImagePath = await storage.SaveAsync(request.ImageFile, ct);
            entity.ImagePath = newImagePath;
        }

        // 2) write the new path to the database
        try
        {
            await ctx.SaveChangesAsync(ct);
        }
        catch
        {
            // the treatment keeps its old image, so the new file is not needed
            if (newImagePath is not null)
                storage.Delete(newImagePath);
            throw;
        }

        // 3) only now remove the old file (if nobody else uses it); a failure here must not fail the request
        if (newImagePath is not null && !string.IsNullOrWhiteSpace(oldImagePath))
            await TryDeleteOldImageAsync(oldImagePath, entity.Id, ct);

        return Unit.Value;
    }

    private async Task TryDeleteOldImageAsync(string oldImagePath, int treatmentId, CancellationToken ct)
    {
        try
        {
            var relative = oldImagePath.TrimStart('/', '\\');
            var withSlash = "/" + relative;

            // keep the file while another treatment or medicine still uses it
            var usedElsewhere =
                await ctx.Treatments.AnyAsync(t => t.Id != treatmentId && (t.ImagePath == relative || t.ImagePath == withSlash), ct) ||
                await ctx.Medicine.AnyAsync(m => m.ImagePath == relative || m.ImagePath == withSlash, ct);

            if (!usedElsewhere)
                storage.Delete(relative);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Old treatment image {Path} could not be removed.", oldImagePath);
        }
    }
}