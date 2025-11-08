using MediCare.Application.Modules.Catalog.Treatments.Commands.Create;

public class CreateTreatmentsCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateTreatmentsCommand, int>
{
    public async Task<int> Handle(CreateTreatmentsCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.ServiceName?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Name is required.");

        // Check if a category with the same name already exists.
        bool exists = await context.Treatments
            .AnyAsync(x => x.ServiceName == normalized, cancellationToken);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        var treatment = new Treatments
        {
            ServiceName = request.ServiceName!.Trim(),
            Description = request.Description,
            TreatmentCategoryId = request.TreatmentCategoryId,
            ImagePath = request.ImagePath,
            Price = request.Price,
            isEnabled = request.isEnabled

        };

        context.Treatments.Add(treatment);
        await context.SaveChangesAsync(cancellationToken);

        return treatment.Id;
    }
}