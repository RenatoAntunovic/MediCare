namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Update;

public sealed class UpdateMedicineCategoryCommandHandler(IAppDbContext ctx)
            : IRequestHandler<UpdateMedicineCategoryCommand, Unit>
{
    public async Task<Unit> Handle(UpdateMedicineCategoryCommand request, CancellationToken ct)
    {
        var entity = await ctx.MedicineCategories
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MarketNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");

        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await ctx.MedicineCategories
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        entity.Name = request.Name.Trim();

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}