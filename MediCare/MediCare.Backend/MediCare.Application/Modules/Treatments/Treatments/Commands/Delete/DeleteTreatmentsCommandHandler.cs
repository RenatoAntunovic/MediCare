namespace MediCare.Application.Modules.Catalog.Treatments.Commands.Delete;

public class DeleteTreatmentsCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
      : IRequestHandler<DeleteTreatmentsCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTreatmentsCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MediCareBusinessRuleException("123", "Korisnik nije autentifikovan.");

        var treatments = await context.Treatments
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (treatments is null)
            throw new MediCareNotFoundException("Tretman nije pronađen.");

        var hasReservations = await context.Reservations
            .AnyAsync(r => r.TreatmentId == treatments.Id, cancellationToken);

        if (hasReservations)
            throw new MediCareBusinessRuleException(
                "treatment.delete.hasReservations",
                "Tretman se ne može obrisati jer ima rezervacije. Umjesto brisanja ga deaktivirajte.");

        treatments.IsDeleted = true; // Soft delete
        treatments.isEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
