namespace MediCare.Application.Modules.Reservations.Common;

/// <summary>
/// Appointment rules in ONE place (used by validators, handlers and the availability check).
/// They must match what the frontend calendar offers.
/// </summary>
public static class ReservationRules
{
    public const string DraftStatus = "DRAFT";
    public const string CancelledStatus = "CANCELLED";
    public const int NotesMaxLength = 1000;

    public static readonly TimeSpan FirstSlot = new(8, 0, 0);
    public static readonly TimeSpan LastSlot = new(17, 0, 0);
    public static readonly TimeSpan SlotLength = TimeSpan.FromMinutes(30);

    /// <summary>08:00, 08:30 ... 17:00</summary>
    public static bool IsValidSlot(TimeSpan time) =>
        time >= FirstSlot &&
        time <= LastSlot &&
        time.Seconds == 0 &&
        time.Milliseconds == 0 &&
        (time.Minutes == 0 || time.Minutes == 30);

    /// <summary>Working day, tomorrow at the earliest.</summary>
    public static bool IsBookableDate(DateTime date) =>
        date.Date > DateTime.Today &&
        date.DayOfWeek != DayOfWeek.Saturday &&
        date.DayOfWeek != DayOfWeek.Sunday;
}
