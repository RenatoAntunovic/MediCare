namespace MediCare.Application.Modules.Procurement.Suppliers.Commands.Status;

public sealed class EnableSupplierCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public sealed class DisableSupplierCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public sealed class SupplierStatusCommandHandler(IAppDbContext ctx)
    : IRequestHandler<EnableSupplierCommand, Unit>,
      IRequestHandler<DisableSupplierCommand, Unit>
{
    public Task<Unit> Handle(EnableSupplierCommand request, CancellationToken ct) => SetStatusAsync(request.Id, true, ct);

    public Task<Unit> Handle(DisableSupplierCommand request, CancellationToken ct) => SetStatusAsync(request.Id, false, ct);

    private async Task<Unit> SetStatusAsync(int id, bool enabled, CancellationToken ct)
    {
        var supplier = await ctx.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new MediCareNotFoundException($"Dobavljač (ID={id}) nije pronađen.");

        if (supplier.IsEnabled != enabled) // idempotent
        {
            supplier.IsEnabled = enabled;
            await ctx.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}
