using MediCare.Application.Modules.MedicineSearch;

namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Enable;

public sealed class EnableMedicineCommandHandler(IAppDbContext ctx, IMedicineSearchIndex searchIndex)
    : IRequestHandler<EnableMedicineCommand, Unit>
{
    public async Task<Unit> Handle(EnableMedicineCommand request, CancellationToken ct)
    {
        var entity = await ctx.Medicine
            .Include(x => x.MedicineCategory)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Lijek (ID={request.Id}) nije pronađen.");

        // Same rule as disabling a category: an active medicine cannot belong to a disabled category
        if (!entity.MedicineCategory.IsEnabled)
            throw new MediCareBusinessRuleException("medicine.category.disabled",
                $"Lijek se ne može aktivirati jer je kategorija {entity.MedicineCategory.Name} neaktivna.");

        if (!entity.isEnabled)
        {
            entity.isEnabled = true;
            await ctx.SaveChangesAsync(ct);
        }

        await searchIndex.SyncMedicineAsync(ctx, entity.Id, ct);

        return Unit.Value;
    }
}
