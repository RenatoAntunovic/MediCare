namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Delete;

public class DeleteMedicineCategoryCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
      : IRequestHandler<DeleteMedicineCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMedicineCategoryCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MediCareBusinessRuleException("123", "User is not autentificated.");

        var category = await context.MedicineCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new MediCareNotFoundException("Category not found.");

        var hasMedicines = await context.Medicine
    .AnyAsync(m => m.MedicineCategoryId == category.Id, cancellationToken);

        if (hasMedicines)
            throw new MediCareBusinessRuleException(
                "category.notEmpty",
                "Category can't be deleted because it contains medicine. First move them to another category or delete.");

        category.IsDeleted = true; // Soft delete
        category.IsEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
