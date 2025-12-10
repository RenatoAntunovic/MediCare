namespace MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Delete;

public class DeleteMedicineCategoryCommandHandler(IAppDbContext context, IAppCurrentUser appCurrentUser)
      : IRequestHandler<DeleteMedicineCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMedicineCategoryCommand request, CancellationToken cancellationToken)
    {
        if (appCurrentUser.UserId is null)
            throw new MarketBusinessRuleException("123", "Korisnik nije autentifikovan.");

        var category = await context.MedicineCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new MarketNotFoundException("Kategorija nije pronađena.");

        category.IsDeleted = true; // Soft delete
        category.IsEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
