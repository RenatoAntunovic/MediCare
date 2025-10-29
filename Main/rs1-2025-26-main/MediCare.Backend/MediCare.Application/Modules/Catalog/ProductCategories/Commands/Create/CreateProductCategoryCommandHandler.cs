namespace MediCare.Application.Modules.Catalog.ProductCategories.Commands.Create;

public class CreateProductCategoryCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateMedicineCategoryCommand, int>
{
    public async Task<int> Handle(CreateMedicineCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Name is required.");

        // Check if a category with the same name already exists.
        bool exists = await context.MedicineCategories
            .AnyAsync(x => x.Name == normalized, cancellationToken);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        var category = new MedicineCategories
        {
            Name = request.Name!.Trim(),
            IsEnabled = true // deault IsEnabled
        };

        context.MedicineCategories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}