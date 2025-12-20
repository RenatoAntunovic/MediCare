namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Status.Disable;

public sealed class DisableTreatmentCategoryCommandHandler(IAppDbContext ctx)
    : IRequestHandler<DisableTreatmentCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DisableTreatmentCategoryCommand request, CancellationToken ct)
    {
        var cat = await ctx.TreatmentCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (cat is null)
        {
            throw new MediCareNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");
        }

        if (!cat.isEnabled) return Unit.Value; // idempotent

        // Business rule: cannot disable if there are active products
        var hasActiveTreatments = await ctx.Treatments
            .AnyAsync(t=>t.TreatmentCategoryId == cat.Id && t.isEnabled, ct);

        if (hasActiveTreatments)
        {
            throw new MediCareBusinessRuleException("category.disable.blocked.activeProducts",
                $"Category {cat.CategoryName} cannot be disabled because it contains active products.");
        }

        cat.isEnabled = false;

        await ctx.SaveChangesAsync(ct);

        // await _bus.PublishAsync(new ProductCategoryDisabledV1IntegrationEvent(cat.Id, ...), ct);
        // await _cache.RemoveAsync(CacheKeys.CategoriesList, ct);

        return Unit.Value;
    }
}