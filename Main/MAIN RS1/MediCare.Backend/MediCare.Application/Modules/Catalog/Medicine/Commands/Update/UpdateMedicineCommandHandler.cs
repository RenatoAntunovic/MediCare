namespace MediCare.Application.Modules.Catalog.Medicine.Commands.Update;

public sealed class UpdateMedicineCommandHandler(IAppDbContext ctx)
            : IRequestHandler<UpdateMedicineCommand, Unit>
{
    public async Task<Unit> Handle(UpdateMedicineCommand request, CancellationToken ct)
    {
        var entity = await ctx.Medicine
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MarketNotFoundException($"Medicine (ID={request.Id}) not found.");
    
        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await ctx.Medicine
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.Price = request.Price;
        entity.Weight = request.Weight;
        entity.MedicineCategoryId = request.MedicineCategoryId;
        entity.ImagePath = request.ImagePath;

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