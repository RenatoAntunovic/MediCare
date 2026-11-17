namespace MediCare.Application.Common;

/// <summary>
/// One definition of "revenue" for orders and reservations, used by the statistics page and the PDF report.
/// Status flow: DRAFT -> CONFIRMED -> PAID -> COMPLETED (or CANCELLED).
/// </summary>
public static class RevenueRules
{
    /// <summary>
    /// Realised revenue = money that was paid: PAID, and COMPLETED (a finished order or appointment was paid before).
    /// DRAFT, CONFIRMED and CANCELLED are not revenue.
    /// </summary>
    public static readonly string[] RealisedStatuses = ["PAID", "COMPLETED"];

    /// <summary>Confirmed but not yet paid. Shown separately, never added to the revenue.</summary>
    public const string ConfirmedStatus = "CONFIRMED";

    public static bool IsRealised(string statusName) => RealisedStatuses.Contains(statusName);

    public static bool IsConfirmedNotPaid(string statusName) => statusName == ConfirmedStatus;
}
