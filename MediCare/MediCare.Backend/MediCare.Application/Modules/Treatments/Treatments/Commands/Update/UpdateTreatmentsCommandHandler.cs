using System.Diagnostics;

namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Update;

public sealed class UpdateTreatmentsCommandHandler(IAppDbContext ctx)
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

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // (optional) delete the old image
            // Delete the old image only if no other treatment or medicine still uses it
            if (!string.IsNullOrWhiteSpace(entity.ImagePath))
            {
                var relative = entity.ImagePath.TrimStart('/', '\\');
                var withSlash = "/" + relative;

                var usedElsewhere =
                    await ctx.Treatments.AnyAsync(t => t.Id != entity.Id && (t.ImagePath == relative || t.ImagePath == withSlash), ct) ||
                    await ctx.Medicine.AnyAsync(m => m.ImagePath == relative || m.ImagePath == withSlash, ct);

                var oldPath = Path.Combine("wwwroot", relative);
                if (!usedElsewhere && File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.ImageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await request.ImageFile.CopyToAsync(stream, ct);

            // ⬅️ The ONLY place where ImagePath is set
            entity.ImagePath = $"images/{uniqueFileName}";
        }

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}