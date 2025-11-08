using MediCare.Application.Modules.Catalog.Medicine.Commands.Create;

public class CreateMedicineCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateMedicineCommand, int>
{
    public async Task<int> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name?.Trim();

        //if (string.IsNullOrWhiteSpace(normalized))
        //    throw new ValidationException("Name is required."); ovo sam zakomentarisao zato sto se sve vec testira u validatoru

        // Check if a category with the same name already exists.
        bool exists = await context.Medicine
            .AnyAsync(x => x.Name == normalized, cancellationToken);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        var medicine = new Medicine
        {
            Name = request.Name!.Trim(),
            Description = request.Description,
            MedicineCategoryId = request.MedicineCategoryId,
            ImagePath = request.ImagePath,
            Weight = request.Weight,
            Price = request.Price,
            isEnabled = request.isEnabled

        };

        context.Medicine.Add(medicine);
        await context.SaveChangesAsync(cancellationToken);

        return medicine.Id;
    }
}