namespace MediCare.Application.Modules.Notifications;

/// <summary>
/// Human-readable (Bosnian) names of order/reservation statuses for notification texts.
/// </summary>
public static class StatusLabels
{
    public static string ToBosnian(string statusName) => statusName switch
    {
        "DRAFT" => "na čekanju",
        "CONFIRMED" => "potvrđena",
        "PAID" => "plaćena",
        "COMPLETED" => "završena",
        "CANCELLED" => "otkazana",
        _ => statusName
    };
}