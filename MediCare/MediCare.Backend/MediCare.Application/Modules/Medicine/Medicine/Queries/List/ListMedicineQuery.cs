namespace MediCare.Application.Modules.Medicine.Medicine.Queries.List;

public sealed class ListMedicineQuery : BasePagedQuery<ListMedicineQueryDto>
{
    public string? Search { get; init; }
    public int? CategoryId { get; set; }

    /// <summary>Status filter, used only by the admin list (other callers always get active medicines).</summary>
    public bool? OnlyEnabled { get; init; }

    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
}

public sealed class ListMedicineQueryValidator : AbstractValidator<ListMedicineQuery>
{
    public ListMedicineQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("Search can be at most 100 characters long.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue)
            .WithMessage("Minimum price cannot be negative.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue)
            .WithMessage("Maximum price cannot be negative.");

        RuleFor(x => x)
            .Must(x => x.MinPrice <= x.MaxPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithName("MaxPrice")
            .WithMessage("Maximum price must be greater than or equal to the minimum price.");
    }
}
