namespace MediCare.Application.Modules.Medicine.Medicine.Commands.Delete;

public class DeleteMedicineCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
      : IRequestHandler<DeleteMedicineCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MediCareBusinessRuleException("123", "User is not authentificated.");

        var medicine = await context.Medicine
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (medicine is null)
            throw new MediCareNotFoundException("Category was not found.");

        medicine.IsDeleted = true; // Soft delete
        medicine.isEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
