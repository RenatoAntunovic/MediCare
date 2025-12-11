namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Enable;

public sealed class EnableTreatmentCategoryCommandHandler(IAppDbContext ctx)
    : IRequestHandler<EnableTreatmentCategoryCommand, Unit>
{
    public async Task<Unit> Handle(EnableTreatmentCategoryCommand request, CancellationToken ct)
    {
        var entity = await ctx.TreatmentCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");

        if (!entity.isEnabled)
        {
            entity.isEnabled = true;
            await ctx.SaveChangesAsync(ct);
        }

        // If already enabled — nothing changes, idempotent
        return Unit.Value;
    }
}