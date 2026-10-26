namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Delete;

public class DeleteMedicineCommandHandler(
    IAppDbContext context,
    IAppCurrentUser appCurrentUser,
    IMedicineSearchIndex searchIndex)
      : IRequestHandler<DeleteMedicineCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MediCareBusinessRuleException("auth.required", "Korisnik nije autentifikovan.");

        var medicine = await context.Medicine
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (medicine is null)
            throw new MediCareNotFoundException("Lijek nije pronađen.");

        // A medicine that was ever ordered stays in the database – otherwise it would vanish from old orders,
        // PDFs and statistics. The admin should disable it instead.
        var wasOrdered = await context.OrderItems
            .AnyAsync(oi => oi.MedicineId == medicine.Id, cancellationToken);

        if (wasOrdered)
            throw new MediCareBusinessRuleException(
                "medicine.delete.hasOrders",
                "Lijek se ne može obrisati jer postoji u narudžbama. Umjesto brisanja ga deaktivirajte.");

        // Never ordered → remove it from every cart, favourites and "for later" list first
        var cartItems = await context.CartItems.Where(ci => ci.MedicineId == medicine.Id).ToListAsync(cancellationToken);
        var favourites = await context.Favourites.Where(f => f.MedicineId == medicine.Id).ToListAsync(cancellationToken);
        var forLater = await context.ForLater.Where(f => f.MedicineId == medicine.Id).ToListAsync(cancellationToken);

        context.CartItems.RemoveRange(cartItems);
        context.Favourites.RemoveRange(favourites);
        context.ForLater.RemoveRange(forLater);

        medicine.IsDeleted = true; // Soft delete
        medicine.isEnabled = false;

        await context.SaveChangesAsync(cancellationToken);

        await searchIndex.RemoveAsync(medicine.Id, cancellationToken);

        return Unit.Value;
    }
}
