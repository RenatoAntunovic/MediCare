namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Status.Disable;

public sealed class DisableMedicineCommandHandler(IAppDbContext ctx, IMedicineSearchIndex searchIndex)
    : IRequestHandler<DisableMedicineCommand, Unit>
{
    public async Task<Unit> Handle(DisableMedicineCommand request, CancellationToken ct)
    {
        var entity = await ctx.Medicine
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MediCareNotFoundException($"Lijek (ID={request.Id}) nije pronađen.");

        if (entity.isEnabled)
        {
            entity.isEnabled = false;
            await ctx.SaveChangesAsync(ct);
        }

        // A disabled medicine must not appear in public search
        await searchIndex.RemoveAsync(entity.Id, ct);

        return Unit.Value;
    }
}
