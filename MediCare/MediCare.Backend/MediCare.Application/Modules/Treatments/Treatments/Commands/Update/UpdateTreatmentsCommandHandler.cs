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
    
        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await ctx.Treatments
            .AnyAsync(x => x.Id != request.Id && x.ServiceName.ToLower() == request.ServiceName.ToLower(), ct);

        if (exists)
        {
            throw new MediCareConflictException("Name already exists.");
        }

        entity.ServiceName = request.ServiceName;
        entity.Price = request.Price;
        entity.TreatmentCategoryId = request.TreatmentCategoryId;
        entity.Description = request.Description;

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













        //if (!string.IsNullOrWhiteSpace(request.Name))
        //{
        //    // Check for duplicate
        //    var exists = await ctx.Medicine
        //        .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

        //    if (exists)
        //        throw new MarketConflictException("Name already exists.");

        //    entity.Name = request.Name.Trim();
        //}

        //if (!string.IsNullOrWhiteSpace(request.Description))
        //    entity.Description = request.Description;

        //if (request.Price.HasValue)
        //    entity.Price = request.Price.Value;

        //if (request.Weight.HasValue)
        //    entity.Weight = request.Weight.Value;

        //if (!string.IsNullOrWhiteSpace(request.ImagePath))
        //    entity.ImagePath = request.ImagePath;

        //if (request.MedicineCategoryId.HasValue)
        //{
        //    entity.MedicineCategoryId = request.MedicineCategoryId.Value;
        //}
    }
}