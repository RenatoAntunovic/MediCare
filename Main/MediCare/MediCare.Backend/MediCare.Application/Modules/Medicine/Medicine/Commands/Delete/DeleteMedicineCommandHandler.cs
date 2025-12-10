namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Delete;

public class DeleteMedicineCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
      : IRequestHandler<DeleteMedicineCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MarketBusinessRuleException("123", "Korisnik nije autentifikovan.");

        var medicine = await context.Medicine
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (medicine is null)
            throw new MarketNotFoundException("Kategorija nije pronađena.");

        medicine.IsDeleted = true; // Soft delete
        medicine.isEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
