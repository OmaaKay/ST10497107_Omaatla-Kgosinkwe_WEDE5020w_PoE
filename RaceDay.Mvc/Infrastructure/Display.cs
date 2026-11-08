using RaceDay.Mvc.Models.Api;

namespace RaceDay.Mvc.Infrastructure;

/// <summary>Small presentation helpers shared by the views.</summary>
public static class Display
{
    /// <summary>1 → "1st", 2 → "2nd", 11 → "11th", 47 → "47th".</summary>
    public static string Ordinal(int number)
    {
        var abs = Math.Abs(number);
        var suffix = (abs % 100) is >= 11 and <= 13
            ? "th"
            : (abs % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
        return number + suffix;
    }

    /// <summary>"47th of 312 finishers", or just "47th" when the total is unknown.</summary>
    public static string PositionText(int? position, int totalFinishers)
    {
        if (position is null) return "—";
        return totalFinishers > 0
            ? $"{Ordinal(position.Value)} out of {totalFinishers} finishers"
            : Ordinal(position.Value);
    }

    public static string FinishTime(TimeOnly? time) =>
        time is null ? "—" : time.Value.ToString("HH:mm:ss");

    public static string Date(DateOnly date) => date.ToString("ddd, d MMM yyyy");

    public static string Time(TimeOnly time) => time.ToString("HH:mm");

    public static string Distance(decimal? km) =>
        km is null ? "—" : $"{km.Value:0.##} km";

    public static string TypeIcon(EventType type) => type switch
    {
        EventType.Run => "bi-person-walking",
        EventType.Walk => "bi-tree",
        EventType.Cycle => "bi-bicycle",
        _ => "bi-flag"
    };

    public static string Initials(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "?";
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1
            ? parts[0][..1].ToUpperInvariant()
            : (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
    }

    /// <summary>CSS class for a status badge (enrolment or event status).</summary>
    public static string StatusClass(string? status) => (status ?? string.Empty).ToLowerInvariant() switch
    {
        "confirmed" or "open" => "status-confirmed",   // green
        "pending" => "status-pending",                 // amber
        "cancelled" => "status-cancelled",             // red
        "completed" => "status-completed",             // blue
        "closed" => "status-closed",                   // grey
        _ => "status-closed"
    };

    public static bool IsUpcoming(EventDto e) =>
        e.EventDate >= DateOnly.FromDateTime(DateTime.Today) &&
        !string.Equals(e.Status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    public static bool RegistrationOpen(EventDto e) =>
        string.Equals(e.Status, "Open", StringComparison.OrdinalIgnoreCase) &&
        e.RegistrationDeadline >= DateOnly.FromDateTime(DateTime.Today);
}
