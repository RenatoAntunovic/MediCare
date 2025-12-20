namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Delete;

public class DeleteTreatmentCategoryCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
      : IRequestHandler<DeleteTreatmentCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTreatmentCategoryCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MediCareBusinessRuleException("123", "Korisnik nije autentifikovan.");

        var category = await context.TreatmentCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new MediCareNotFoundException("Kategorija nije pronađena.");

        category.IsDeleted = true; // Soft delete
        category.isEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
