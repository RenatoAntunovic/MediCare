namespace MediCare.Application.Modules.Catalog.TreatmentCategories.Queries.List;

public sealed class ListTreatmentCategoriesQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListTreatmentCategoriesQuery, PageResult<ListTreatmentCategoriesQueryDto>>
{
    public async Task<PageResult<ListTreatmentCategoriesQueryDto>> Handle(
        ListTreatmentCategoriesQuery request, CancellationToken ct)
    {
        var q = ctx.TreatmentCategories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
             q = q.Where(x => x.CategoryName.Contains(request.Search));
        }

        if (request.OnlyEnabled is not null)
            q = q.Where(x => x.isEnabled == request.OnlyEnabled);

        var projectedQuery = q.OrderBy(x => x.CategoryName)
            .Select(x => new ListTreatmentCategoriesQueryDto
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                isEnabled = x.isEnabled
            });

        return await PageResult<ListTreatmentCategoriesQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
