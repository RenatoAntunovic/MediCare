namespace MediCare.Application.Common;

/// <summary>
/// Base class for list queries with pagination, search, and sorting.
/// </summary>
public abstract class BasePagedQuery<TItem> : IRequest<PageResult<TItem>>
{
    /// <summary>Pagination parameters (page number and page size).</summary>
    public PageRequest Paging { get; init; } = new();

    /// <summary>Column to sort by (e.g. "name", "price"). Each handler decides which columns it supports.</summary>
    public string? SortBy { get; init; }

    /// <summary>"asc" or "desc" (default "asc").</summary>
    public string? SortDirection { get; init; }

    /// <summary>True when SortDirection is "desc". A method (not a property) so it isn't shown as a query parameter.</summary>
    public bool IsSortDescending() => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}