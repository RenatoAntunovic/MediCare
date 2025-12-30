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

        var projectedQuery = q.OrderBy(x => x.ServiceName)
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
