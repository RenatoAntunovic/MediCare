namespace MediCare.Application.Modules.Medicine.Medicine.Queries.List;

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

        if (request.CategoryId.HasValue) 
            q = q.Where(x => x.MedicineCategoryId == request.CategoryId.Value);

        // Sort ALL rows on the server before paging (the table sends sortBy + sortDirection)
        var desc = request.IsSortDescending();
        var ordered = (request.SortBy ?? "").ToLowerInvariant() switch
        {
            "price" => desc ? q.OrderByDescending(x => x.Price) : q.OrderBy(x => x.Price),
            "weight" => desc ? q.OrderByDescending(x => x.Weight) : q.OrderBy(x => x.Weight),
            "medicinecategoryname" => desc ? q.OrderByDescending(x => x.MedicineCategory.Name) : q.OrderBy(x => x.MedicineCategory.Name),
            "isenabled" => desc ? q.OrderByDescending(x => x.isEnabled) : q.OrderBy(x => x.isEnabled),
            _ => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name)
        };

        var projectedQuery = ordered.ThenBy(x => x.Id) // stable order between pages
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
