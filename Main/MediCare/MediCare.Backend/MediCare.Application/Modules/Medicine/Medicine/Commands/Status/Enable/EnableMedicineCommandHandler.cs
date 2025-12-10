namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Enable;

public sealed class EnableMedicineCommandHandler(IAppDbContext ctx)
    : IRequestHandler<EnableMedicineCommand, Unit>
{
    public async Task<Unit> Handle(EnableMedicineCommand request, CancellationToken ct)
    {
        var entity = await ctx.Medicine
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException($"Medicina (ID={request.Id}) nije pronađena.");

        if (!entity.isEnabled)
        {
            entity.isEnabled = true;
            await ctx.SaveChangesAsync(ct);
        }

        // If already enabled — nothing changes, idempotent
        return Unit.Value;
    }
}