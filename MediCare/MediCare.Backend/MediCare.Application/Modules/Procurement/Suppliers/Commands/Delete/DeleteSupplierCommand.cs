namespace MediCare.Application.Modules.Procurement.Suppliers.Commands.Delete;

public sealed class DeleteSupplierCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public sealed class DeleteSupplierCommandHandler(IAppDbContext ctx)
    : IRequestHandler<DeleteSupplierCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSupplierCommand request, CancellationToken ct)
    {
        var supplier = await ctx.Suppliers
            .Include(s => s.MedicineSuppliers)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new MediCareNotFoundException($"Dobavljač (ID={request.Id}) nije pronađen.");

        // Soft delete: the row stays for history, the global query filter hides it
        supplier.IsDeleted = true;
        supplier.IsEnabled = false;

        foreach (var link in supplier.MedicineSuppliers)
            link.IsDeleted = true;

        await ctx.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
