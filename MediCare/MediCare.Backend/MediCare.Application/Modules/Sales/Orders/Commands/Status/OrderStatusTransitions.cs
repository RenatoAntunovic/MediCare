namespace MediCare.Application.Modules.Sales.Orders.Commands.Status;

/// <summary>
/// Allowed order status transitions (the same rule the admin change-status dialog shows):
/// DRAFT → CONFIRMED → PAID → COMPLETED, and CANCELLED from any status before COMPLETED.
/// COMPLETED and CANCELLED are final.
/// </summary>
public static class OrderStatusTransitions
{
    private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DRAFT"] = ["CONFIRMED", "CANCELLED"],
        ["CONFIRMED"] = ["PAID", "CANCELLED"],
        ["PAID"] = ["COMPLETED", "CANCELLED"],
        ["COMPLETED"] = [],
        ["CANCELLED"] = []
    };

    public static bool IsAllowed(string currentStatus, string newStatus) =>
        Allowed.TryGetValue(currentStatus, out var next) &&
        next.Contains(newStatus, StringComparer.OrdinalIgnoreCase);
}
