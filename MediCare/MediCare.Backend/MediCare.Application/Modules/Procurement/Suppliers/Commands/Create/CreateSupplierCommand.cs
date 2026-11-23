using SupplierEntity = MediCare.Domain.Entities.HospitalRecords.Suppliers;


namespace MediCare.Application.Modules.Procurement.Suppliers.Commands.Create;

public sealed class CreateSupplierCommand : SupplierFields, IRequest<int>
{
}

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        Include(new SupplierFieldsValidator());
    }
}

public sealed class CreateSupplierCommandHandler(IAppDbContext ctx)
    : IRequestHandler<CreateSupplierCommand, int>
{
    public async Task<int> Handle(CreateSupplierCommand request, CancellationToken ct)
    {
        await SupplierRules.EnsureUniqueAsync(ctx, request, null, ct);
        var medicineIds = await SupplierRules.ValidMedicineIdsAsync(ctx, request.MedicineIds, ct);

        var supplier = new SupplierEntity();
        SupplierRules.Apply(supplier, request);

        foreach (var medicineId in medicineIds)
            supplier.MedicineSuppliers.Add(new MedicineSuppliers { MedicineId = medicineId });

        ctx.Suppliers.Add(supplier);
        await ctx.SaveChangesAsync(ct);

        return supplier.Id;
    }
}
