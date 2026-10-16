namespace MediCare.Application.Modules.Statistics.Queries.Dashboard;

/// <summary>
/// Data for the admin statistics page (charts).
/// No From/To → all data. LowStockThreshold decides which medicines count as "low stock".
/// </summary>
public sealed class StatisticsDashboardQuery : IRequest<StatisticsDashboardDto>
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int LowStockThreshold { get; init; } = 10;
}

public sealed class StatisticsDashboardQueryValidator : AbstractValidator<StatisticsDashboardQuery>
{
    public StatisticsDashboardQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From.Value.Date <= x.To.Value.Date)
            .WithMessage("Datum 'od' ne može biti nakon datuma 'do'.");

        RuleFor(x => x.LowStockThreshold)
            .InclusiveBetween(0, 1000)
            .WithMessage("Prag zaliha mora biti između 0 i 1000.");
    }
}
