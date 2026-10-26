namespace MediCare.Application.Modules.MedicineSearch;

public sealed class SearchMedicinesQuery : IRequest<MedicineSearchResultDto>
{
    public string Query { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class SearchMedicinesQueryValidator : AbstractValidator<SearchMedicinesQuery>
{
    public SearchMedicinesQueryValidator()
    {
        // No minimum length here: the client medicine list may search by a single letter.
        // (The public landing page enforces its own 2-character minimum on the frontend.)
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Unesite pojam za pretragu.")
            .MaximumLength(100).WithMessage("Pojam za pretragu može imati najviše 100 znakova.");

        RuleFor(x => x.Page).InclusiveBetween(1, 10000);

        // Not capped here on purpose: the handler clamps the page size instead of rejecting the request,
        // because the client list sends 100 (or the 1000 default) as page size.
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public sealed class SearchMedicinesQueryHandler(IMedicineSearchService search)
    : IRequestHandler<SearchMedicinesQuery, MedicineSearchResultDto>
{
    private const int MaxPageSize = 1000;

    public Task<MedicineSearchResultDto> Handle(SearchMedicinesQuery request, CancellationToken ct)
        => search.SearchAsync(request.Query.Trim(), request.Page, Math.Min(request.PageSize, MaxPageSize), ct);
}
