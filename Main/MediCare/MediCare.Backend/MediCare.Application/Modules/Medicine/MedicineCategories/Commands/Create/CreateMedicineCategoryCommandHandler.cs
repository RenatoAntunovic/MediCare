using MediCare.Application.Modules.Medicine.MedicineCategories.Commands.Create;

public class CreateMedicineCategoryCommandHandler(IAppDbContext context)
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
            throw new MediCareConflictException("Name already exists.");
        }

        var medicineCategory = new MedicineCategories
        {
            Name = request.Name!.Trim()
        };

        context.MedicineCategories.Add(medicineCategory);
        await context.SaveChangesAsync(cancellationToken);

        return medicineCategory.Id;
    }
}