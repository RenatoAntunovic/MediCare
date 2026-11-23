namespace MediCare.Application.Modules.Procurement.Suppliers.Queries.List;

/// <summary>
/// Supplier list with 5 filters, all applied on the server BEFORE counting and paging:
/// text (company, contact person, email), city, status, supplied medicine and "added from" date.
/// </summary>
public sealed class ListSuppliersQuery : BasePagedQuery<ListSuppliersQueryDto>
{
    public string? Search { get; init; }
    public string? City { get; init; }
    public bool? OnlyEnabled { get; init; }
    public int? MedicineId { get; init; }
    public DateTime? CreatedFrom { get; init; }
}

public sealed class ListSuppliersQueryDto
{
    public required int Id { get; init; }
    public required string CompanyName { get; init; }
    public required string ContactName { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required string City { get; init; }
    public required bool IsEnabled { get; init; }
    public required int MedicinesCount { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}

public sealed class ListSuppliersQueryValidator : AbstractValidator<ListSuppliersQuery>
{
    public ListSuppliersQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("Search can be at most 100 characters long.");
        RuleFor(x => x.City).MaximumLength(50).WithMessage("City can be at most 50 characters long.");
        RuleFor(x => x.MedicineId).GreaterThan(0).When(x => x.MedicineId.HasValue);
        RuleFor(x => x.CreatedFrom)
            .Must(d => d!.Value.Date <= DateTime.UtcNow.Date.AddDays(1))
            .When(x => x.CreatedFrom.HasValue)
            .WithMessage("The \"added from\" date cannot be in the future.");
    }
}

public sealed class ListSuppliersQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListSuppliersQuery, PageResult<ListSuppliersQueryDto>>
{
    public async Task<PageResult<ListSuppliersQueryDto>> Handle(ListSuppliersQuery request, CancellationToken ct)
    {
        var q = ctx.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(s =>
                s.CompanyName.Contains(term) ||
                s.ContactName.Contains(term) ||
                s.Email.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim();
            q = q.Where(s => s.City == city);
        }

        if (request.OnlyEnabled.HasValue)
            q = q.Where(s => s.IsEnabled == request.OnlyEnabled.Value);

        if (request.MedicineId.HasValue)
            q = q.Where(s => s.MedicineSuppliers.Any(l => l.MedicineId == request.MedicineId.Value));

        if (request.CreatedFrom.HasValue)
        {
            var from = request.CreatedFrom.Value.Date;
            q = q.Where(s => s.CreatedAtUtc >= from);
        }

        var desc = request.IsSortDescending();
        var ordered = (request.SortBy ?? "").ToLowerInvariant() switch
        {
            "city" => desc ? q.OrderByDescending(s => s.City) : q.OrderBy(s => s.City),
            "contactname" => desc ? q.OrderByDescending(s => s.ContactName) : q.OrderBy(s => s.ContactName),
            "createdatutc" => desc ? q.OrderByDescending(s => s.CreatedAtUtc) : q.OrderBy(s => s.CreatedAtUtc),
            _ => desc ? q.OrderByDescending(s => s.CompanyName) : q.OrderBy(s => s.CompanyName)
        };

        var projected = ordered.ThenBy(s => s.Id).Select(s => new ListSuppliersQueryDto
        {
            Id = s.Id,
            CompanyName = s.CompanyName,
            ContactName = s.ContactName,
            Email = s.Email,
            Phone = s.Phone,
            City = s.City,
            IsEnabled = s.IsEnabled,
            MedicinesCount = s.MedicineSuppliers.Count(),
            CreatedAtUtc = s.CreatedAtUtc
        });

        return await PageResult<ListSuppliersQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}

/// <summary>Distinct cities for the city filter dropdown.</summary>
public sealed class ListSupplierCitiesQuery : IRequest<List<string>>
{
}

public sealed class ListSupplierCitiesQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListSupplierCitiesQuery, List<string>>
{
    public Task<List<string>> Handle(ListSupplierCitiesQuery request, CancellationToken ct) =>
        ctx.Suppliers.AsNoTracking()
            .Where(s => s.City != "")
            .Select(s => s.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
}
