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

        medicine.IsDeleted = true; // Soft delete
        medicine.isEnabled = false;

        await context.SaveChangesAsync(cancellationToken);

        await searchIndex.RemoveAsync(medicine.Id, cancellationToken);

        return Unit.Value;
    }
}
