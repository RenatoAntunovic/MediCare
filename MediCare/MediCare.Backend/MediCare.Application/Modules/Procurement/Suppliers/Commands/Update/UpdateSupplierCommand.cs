namespace MediCare.Application.Modules.Procurement.Suppliers.Commands.Update;

public sealed class UpdateSupplierCommand : SupplierFields, IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
}

public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        Include(new SupplierFieldsValidator());
    }
}

public sealed class UpdateSupplierCommandHandler(IAppDbContext ctx)
    : IRequestHandler<UpdateSupplierCommand, Unit>
{
    public async Task<Unit> Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await ctx.Suppliers
            .Include(s => s.MedicineSuppliers)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new MediCareNotFoundException($"Dobavljač (ID={request.Id}) nije pronađen.");

        await SupplierRules.EnsureUniqueAsync(ctx, request, request.Id, ct);
        var medicineIds = await SupplierRules.ValidMedicineIdsAsync(ctx, request.MedicineIds, ct);

        SupplierRules.Apply(supplier, request);

        // Sync the links: remove the ones that were unselected, add the newly selected ones
        foreach (var link in supplier.MedicineSuppliers.Where(l => !medicineIds.Contains(l.MedicineId)).ToList())
            ctx.MedicineSuppliers.Remove(link);

        var linked = supplier.MedicineSuppliers.Select(l => l.MedicineId).ToHashSet();
        foreach (var medicineId in medicineIds.Where(id => !linked.Contains(id)))
            supplier.MedicineSuppliers.Add(new MedicineSuppliers { MedicineId = medicineId });

        await ctx.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
