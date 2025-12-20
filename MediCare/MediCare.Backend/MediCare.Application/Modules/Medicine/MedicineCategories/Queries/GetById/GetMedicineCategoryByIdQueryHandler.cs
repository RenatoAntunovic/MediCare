namespace MediCare.Application.Modules.Medicine.MedicineCategories.Queries.GetById;

public class GetMedicineCategoryByIdQueryHandler(IAppDbContext context) : IRequestHandler<GetMedicineCategoryByIdQuery, GetMedicineCategoryByIdQueryDto>
{
    public async Task<GetMedicineCategoryByIdQueryDto> Handle(GetMedicineCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await context.MedicineCategories
            .Where(c => c.Id == request.Id)
            .Select(x => new GetMedicineCategoryByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                IsEnabled = x.IsEnabled
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (category == null)
        {
            throw new MediCareNotFoundException($"Product category with Id {request.Id} not found.");
        }

        return category;
    }
}