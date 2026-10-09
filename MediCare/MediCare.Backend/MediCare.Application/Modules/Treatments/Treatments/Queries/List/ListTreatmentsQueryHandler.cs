namespace MediCare.Application.Modules.Catalog.Treatments.Queries.List;

public sealed class ListTreatmentsQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListTreatmentsQuery, PageResult<ListTreatmentsQueryDto>>
{
    public async Task<PageResult<ListTreatmentsQueryDto>> Handle(
        ListTreatmentsQuery request, CancellationToken ct)
    {
        var q = ctx.Treatments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            q = q.Where(x => x.ServiceName.Contains(request.Search));
        }

        if (request.OnlyEnabled is not null)
            q = q.Where(x => x.isEnabled == request.OnlyEnabled);

        // Sort ALL rows on the server before paging (the table sends sortBy + sortDirection)
        var desc = request.IsSortDescending();
        var ordered = (request.SortBy ?? "").ToLowerInvariant() switch
        {
            "price" => desc ? q.OrderByDescending(x => x.Price) : q.OrderBy(x => x.Price),
            "treatmentscategoryname" => desc ? q.OrderByDescending(x => x.TreatmentCategory.CategoryName) : q.OrderBy(x => x.TreatmentCategory.CategoryName),
            "isenabled" => desc ? q.OrderByDescending(x => x.isEnabled) : q.OrderBy(x => x.isEnabled),
            _ => desc ? q.OrderByDescending(x => x.ServiceName) : q.OrderBy(x => x.ServiceName)
        };

        var projectedQuery = ordered.ThenBy(x => x.Id) // stable order between pages
            .Select(x => new ListTreatmentsQueryDto
            {
                Id = x.Id,
                ServiceName = x.ServiceName,
                Price = x.Price,
                Description = x.Description,
                TreatmentsCategoryId = x.TreatmentCategoryId,
                TreatmentsCategoryName = x.TreatmentCategory.CategoryName,
                ImagePath = x.ImagePath,
                isEnabled = x.isEnabled,
            });

        return await PageResult<ListTreatmentsQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
