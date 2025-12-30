namespace MediCare.Application.Modules.Medicine.MedicineCategories.Queries.List;

public sealed class ListMedicineCategoriesQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListMedicineCategoriesQuery, PageResult<ListMedicineCategoriesQueryDto>>
{
    public async Task<PageResult<ListMedicineCategoriesQueryDto>> Handle(
        ListMedicineCategoriesQuery request, CancellationToken ct)
    {
        var q = ctx.MedicineCategories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
             q = q.Where(x => x.Name.Contains(request.Search));
        }

        if (request.OnlyEnabled is not null)
            q = q.Where(x => x.IsEnabled == request.OnlyEnabled);

        var projectedQuery = q.OrderBy(x => x.Name)
            .Select(x => new ListMedicineCategoriesQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                IsEnabled = x.IsEnabled
            });

        return await PageResult<ListMedicineCategoriesQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
