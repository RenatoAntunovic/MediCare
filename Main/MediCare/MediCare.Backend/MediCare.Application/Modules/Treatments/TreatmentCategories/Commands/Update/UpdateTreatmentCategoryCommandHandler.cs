namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Update;

public sealed class UpdateTreatmentCategoryCommandHandler(IAppDbContext ctx)
            : IRequestHandler<UpdateTreatmentCategoryCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTreatmentCategoryCommand request, CancellationToken ct)
    {
        var entity = await ctx.TreatmentCategories
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");

        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await ctx.MedicineCategories
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists)
        {
            throw new MediCareConflictException("Name already exists.");
        }

        entity.CategoryName = request.Name.Trim();

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}