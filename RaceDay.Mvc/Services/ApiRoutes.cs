namespace RaceDay.Mvc.Services;

// Every route here matches a controller action in RaceDay.Api.
public static class ApiRoutes
{
    public const string Register = "api/auth/register";
    public const string Login = "api/auth/login";
    public const string Logout = "api/auth/logout";

    public const string Events = "api/events";
    public const string MyEvents = "api/events/mine";
    public static string Event(int id) => $"api/events/{id}";
    public static string EventBanner(int id) => $"api/events/{id}/banner";
    public static string EventResults(int eventId) => $"api/events/{eventId}/results";

    public static string Categories(int eventId) => $"api/events/{eventId}/categories";
    public static string Category(int categoryId) => $"api/categories/{categoryId}";

    // GET lists an event's enrolments (Organiser). POST enters the event (Participant).
    public static string EventEnrolments(int eventId) => $"api/events/{eventId}/enrolments";
    public const string MyEnrolments = "api/users/me/enrolments";

    public static string Result(int enrolmentId) => $"api/enrolments/{enrolmentId}/result";
    public const string MyResults = "api/users/me/results";

    public const string Profile = "api/users/me";
    public const string ProfileImage = "api/users/me/image";
}
