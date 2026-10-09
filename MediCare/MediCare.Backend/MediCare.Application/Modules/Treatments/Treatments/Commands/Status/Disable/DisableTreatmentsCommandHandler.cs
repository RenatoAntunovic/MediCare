using MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Disable;

namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Disable;

public sealed class DisableTreatmentsCommandHandler(IAppDbContext ctx)
    : IRequestHandler<DisableTreatmentsCommand, Unit>
{
    public async Task<Unit> Handle(DisableTreatmentsCommand request, CancellationToken ct)
    {
        //var cat = await ctx.Medicine
        //    .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        //if (cat is null)
        //{
        //    throw new MarketNotFoundException($"Category (ID={request.Id}) not found.");
        //}

        //if (!cat.isEnabled) return Unit.Value; // idempotent

        //// Business rule: cannot disable if there are active products
        //var hasActiveProducts = await ctx.Medicine
        //    .AnyAsync(p => p.Id == cat.Id && p.isEnabled, ct);

        //if (hasActiveProducts)
        //{
        //    throw new MarketBusinessRuleException("category.disable.blocked.activeProducts",
        //        $"Medicine {cat.Name} cannot be disabled because!!!!!.");
        //}

        //cat.isEnabled = false;

        //await ctx.SaveChangesAsync(ct);

        //// await _bus.PublishAsync(new ProductCategoryDisabledV1IntegrationEvent(cat.Id, ...), ct);
        //// await _cache.RemoveAsync(CacheKeys.CategoriesList, ct);

        //return Unit.Value;




        //This works, the code above does not – the code above checks e.g. when you deactivate a category
        //that has active medicines it won't allow the deactivation, but I just tried it on MedicineCategory and it still doesn't work
        //Probably it doesn't work as intended because a child (Medicine) still exists even though it is not active
        //Another possibility is that the foreign keys for Medicine and Categories are not set up correctly in the database
        var trea = await ctx.Treatments.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (trea == null) return Unit.Value;

        trea.isEnabled = false;
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}