using MediCare.Application.Modules.Catalog.Treatments.Queries.GetById;

namespace MediCare.Application.Modules.Catalog.Treatments.Queries.GetById;

public class GetTreatmentsByIdQueryHandler(IAppDbContext context) : IRequestHandler<GetTreatmentsByIdQuery, GetTreatmentsByIdQueryDto>
{
    public async Task<GetTreatmentsByIdQueryDto> Handle(GetTreatmentsByIdQuery request, CancellationToken cancellationToken)
    {
        var treatments = await context.Treatments
            .Where(c => c.Id == request.Id)
            .Select(x => new GetTreatmentsByIdQueryDto
            {
                Id = x.Id,
                ServiceName = x.ServiceName,
                Price = x.Price,
                Description = x.Description,
                TreatmentsCategoryId = x.TreatmentCategoryId,
                TreatmentsCategoryName = x.TreatmentCategory.CategoryName,
                ImagePath = x.ImagePath,        
                isEnabled = x.isEnabled,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (treatments == null)
        {
            throw new MarketNotFoundException($"Product with Id {request.Id} not found.");
        }

        return treatments;
    }
}