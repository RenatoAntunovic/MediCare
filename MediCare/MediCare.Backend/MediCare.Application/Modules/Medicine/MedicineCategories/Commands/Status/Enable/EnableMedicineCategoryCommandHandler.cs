namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Status.Enable;

public sealed class EnableMedicineCategoryCommandHandler(IAppDbContext ctx)
    : IRequestHandler<EnableMedicineCategoryCommand, Unit>
{
    public async Task<Unit> Handle(EnableMedicineCategoryCommand request, CancellationToken ct)
    {
        var entity = await ctx.MedicineCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");

        if (!entity.IsEnabled)
        {
            entity.IsEnabled = true;
            await ctx.SaveChangesAsync(ct);
        }

        // If already enabled — nothing changes, idempotent
        return Unit.Value;
    }
}