namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Disable;

public sealed class DisableMedicineCommandHandler(IAppDbContext ctx)
    : IRequestHandler<DisableMedicineCommand, Unit>
{
    public async Task<Unit> Handle(DisableMedicineCommand request, CancellationToken ct)
    {
        //var cat = await ctx.Medicine
        //    .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        //if (cat is null)
        //{
        //    throw new MarketNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");
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




        //Ovo radi ovo gore ne radi fazon je sto gore ima provjeravanje npr ako deaktiviras kategoriju
        //koja ima aktivne medicinne nece dozvoliti deaktivaciju iako sam sad to probao na MedicineCategory i opet ne radi
        //Vjerovatno ne zeli da radi kako treba zato sto ipak postoji child(Medicine) iako nisu aktivni
        //Druga opcija jest da FK u bazi nisu ispravno postavljeni za Medicinu i Kategorije
        var cat = await ctx.Medicine.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (cat == null) return Unit.Value;

        cat.isEnabled = false;
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}