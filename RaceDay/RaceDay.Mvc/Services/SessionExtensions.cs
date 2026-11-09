namespace RaceDay.Mvc.Services;

public static class SessionExtensions
{
    public static bool IsLoggedIn(this ISession session) =>
        session.GetInt32("UserId").HasValue;

    public static bool IsRole(this ISession session, string role) =>
        string.Equals(session.GetString("Role"), role, StringComparison.OrdinalIgnoreCase);
}
