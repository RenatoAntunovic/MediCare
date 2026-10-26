namespace MediCare.Application.Modules.Sales.Orders.Queries.Report;

/// <summary>
/// Parameters of the orders report. All are optional:
/// no From/To → all orders, no StatusId → all statuses.
/// </summary>
public sealed class OrdersReportQuery : IRequest<OrdersReportDto>
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int? StatusId { get; init; }
}

public sealed class OrdersReportQueryValidator : AbstractValidator<OrdersReportQuery>
{
    public OrdersReportQueryValidator()
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
