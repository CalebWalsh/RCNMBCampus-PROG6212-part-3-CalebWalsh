namespace RaceDay.Mvc.Services;

public static class Display
{
    public static string Time(TimeSpan? time) =>
        time is null ? "-" : $"{(int)time.Value.TotalHours:00}:{time.Value.Minutes:00}:{time.Value.Seconds:00}";

    public static string Ordinal(int n)
    {
        string suffix = (n % 100) is >= 11 and <= 13
            ? "th"
            : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
        return n + suffix;
    }

    public static string Position(int? position, int? total)
    {
        if (position is null) return "-";
        return total is > 0 ? $"{Ordinal(position.Value)} of {total}" : Ordinal(position.Value);
    }

    public static string StatusClass(string? status) =>
        (status ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "confirmed" => "status-confirmed",
            "completed" => "status-completed",
            "pending" or "awaiting result" => "status-pending",
            "cancelled" or "canceled" or "rejected" => "status-cancelled",
            _ => "status-other"
        };

    public static string TypeIcon(string? type) =>
        (type ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "running" or "run" => "🏃",
            "walking" or "walk" => "🚶",
            "cycling" or "cycle" => "🚴",
            _ => "🏁"
        };

    public static string Date(DateTime? date) =>
        date is null ? "-" : date.Value.ToString("ddd d MMM yyyy");

    public static string Distance(decimal km) =>
        km % 1 == 0 ? ((int)km).ToString() : km.ToString("0.##");
}
