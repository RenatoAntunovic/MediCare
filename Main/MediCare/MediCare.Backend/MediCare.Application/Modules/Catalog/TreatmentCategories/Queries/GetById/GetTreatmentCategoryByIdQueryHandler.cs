namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.GetById;

public class GetTreatmentCategoryByIdQueryHandler(IAppDbContext context) : IRequestHandler<GetTreatmentCategoryByIdQuery, GetTreatmentCategoryByIdQueryDto>
{
    public async Task<GetTreatmentCategoryByIdQueryDto> Handle(GetTreatmentCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await context.TreatmentCategories
            .Where(c => c.Id == request.Id)
            .Select(x => new GetTreatmentCategoryByIdQueryDto
            {
                Id = x.Id,
                Name = x.CategoryName,
                IsEnabled = x.isEnabled
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (category == null)
        {
            throw new MarketNotFoundException($"Product category with Id {request.Id} not found.");
        }

        return category;
    }
}