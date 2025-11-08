namespace MediCare.Application.Modules.Catalog.Medicine.Queries.List;

public sealed class ListMedicineQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListMedicineQuery, PageResult<ListMedicineQueryDto>>
{
    public async Task<PageResult<ListMedicineQueryDto>> Handle(
        ListMedicineQuery request, CancellationToken ct)
    {
        var q = ctx.Medicine.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            q = q.Where(x => x.Name.Contains(request.Search));
        }

        if (request.OnlyEnabled is not null)
            q = q.Where(x => x.isEnabled == request.OnlyEnabled);

        var projectedQuery = q.OrderBy(x => x.Name)
            .Select(x => new ListMedicineQueryDto
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
            });

        return await PageResult<ListMedicineQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
