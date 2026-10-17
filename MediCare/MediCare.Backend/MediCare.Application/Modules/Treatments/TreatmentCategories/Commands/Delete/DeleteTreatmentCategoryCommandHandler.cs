namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Delete;

public class DeleteTreatmentCategoryCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
      : IRequestHandler<DeleteTreatmentCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTreatmentCategoryCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MediCareBusinessRuleException("123", "Usere isn't autentificated.");

        var category = await context.TreatmentCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new MediCareNotFoundException("Category not found.");

        var hasTreatments = await context.Treatments
    .AnyAsync(t => t.TreatmentCategoryId == category.Id, cancellationToken);

        if (hasTreatments)
            throw new MediCareBusinessRuleException(
                "category.notEmpty",
               "Category can't be deleted because it contains medicine. First move them to another category or delete.");

        category.IsDeleted = true; // Soft delete
        category.isEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
