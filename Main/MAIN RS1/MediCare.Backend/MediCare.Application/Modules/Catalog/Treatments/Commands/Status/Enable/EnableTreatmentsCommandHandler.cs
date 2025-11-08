using MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Enable;

namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Status.Enable;

public sealed class EnableTreatmentsCommandHandler(IAppDbContext ctx)
    : IRequestHandler<EnableTreatmentsCommand, Unit>
{
    public async Task<Unit> Handle(EnableTreatmentsCommand request, CancellationToken ct)
    {
        var entity = await ctx.Treatments
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException($"Tretman (ID={request.Id}) nije pronađena.");

        if (!entity.isEnabled)
        {
            entity.isEnabled = true;
            await ctx.SaveChangesAsync(ct);
        }

        // If already enabled — nothing changes, idempotent
        return Unit.Value;
    }
}