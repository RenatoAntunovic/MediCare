using MediCare.Application.Modules.Sales.Orders.Queries.Report;

namespace MediCare.Application.Modules.Sales.Orders.Queries.MyReport;

/// <summary>
/// Parameters of the "My orders" PDF report for the logged-in client.
/// All are optional: no From/To → all orders, no StatusId → all statuses.
/// </summary>
public sealed class MyOrdersReportQuery : IRequest<MyOrdersReportDto>
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int? StatusId { get; init; }

    /// <summary>When true, every order is listed together with its items.</summary>
    public bool IncludeItems { get; init; } = true;
}

public sealed class MyOrdersReportQueryValidator : AbstractValidator<MyOrdersReportQuery>
{
    public MyOrdersReportQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From.Value.Date <= x.To.Value.Date)
            .WithMessage("Datum 'od' ne može biti nakon datuma 'do'.");

        RuleFor(x => x.StatusId)
            .GreaterThan(0)
            .When(x => x.StatusId.HasValue)
            .WithMessage("Neispravan status.");
    }
}