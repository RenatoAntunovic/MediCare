using MediCare.Application.Modules.Catalog.TreatmentCategories.Commands.Create;

public class CreateTreatmentCategoryCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateTreatmentCategoryCommand, int>
{
    public async Task<int> Handle(CreateTreatmentCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Name is required.");

        // Check if a category with the same name already exists.
        bool exists = await context.TreatmentCategories
            .AnyAsync(x => x.CategoryName == normalized, cancellationToken);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        var treatmentCategory = new TreatmentCategories
        {
            CategoryName = request.Name!.Trim()
        };

        context.TreatmentCategories.Add(treatmentCategory);
        await context.SaveChangesAsync(cancellationToken);

        return treatmentCategory.Id;
    }
}