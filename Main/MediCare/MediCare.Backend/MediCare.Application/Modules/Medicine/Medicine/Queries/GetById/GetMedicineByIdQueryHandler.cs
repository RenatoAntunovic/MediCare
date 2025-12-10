namespace MediCare.Application.Modules.Medicine.Medicine.Queries.GetById;

public class GetMedicineByIdQueryHandler(IAppDbContext context) : IRequestHandler<GetMedicineByIdQuery, GetMedicineByIdQueryDto>
{
    public async Task<GetMedicineByIdQueryDto> Handle(GetMedicineByIdQuery request, CancellationToken cancellationToken)
    {
        var medicine = await context.Medicine
            .Where(c => c.Id == request.Id)
            .Select(x => new GetMedicineByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                Description = x.Description,
                MedicineCategoryId = x.MedicineCategoryId,
                MedicineCategoryName = x.MedicineCategory.Name,
                ImagePath = x.ImagePath,
                Weight = x.Weight,
                isEnabled = x.isEnabled,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (medicine == null)
        {
            throw new MarketNotFoundException($"Product with Id {request.Id} not found.");
        }

        return medicine;
    }
}