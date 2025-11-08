namespace MediCare.Application.Modules.Catalog.MedicineCategories.Commands.Status.Disable;

public sealed class DisableTreatmentCategoryCommandHandler(IAppDbContext ctx)
    : IRequestHandler<DisableMedicineCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DisableMedicineCategoryCommand request, CancellationToken ct)
    {
        var cat = await ctx.MedicineCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (cat is null)
        {
            throw new MarketNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");
        }

        if (!cat.IsEnabled) return Unit.Value; // idempotent

        // Business rule: cannot disable if there are active products
        var hasActiveProducts = await ctx.MedicineCategories
            .AnyAsync(p => p.Id == cat.Id && p.IsEnabled, ct);

        if (hasActiveProducts)
        {
            throw new MarketBusinessRuleException("category.disable.blocked.activeProducts",
                $"Category {cat.Name} cannot be disabled because it contains active products.");
        }

        cat.IsEnabled = false;

        await ctx.SaveChangesAsync(ct);

        // await _bus.PublishAsync(new ProductCategoryDisabledV1IntegrationEvent(cat.Id, ...), ct);
        // await _cache.RemoveAsync(CacheKeys.CategoriesList, ct);

        return Unit.Value;
    }
}