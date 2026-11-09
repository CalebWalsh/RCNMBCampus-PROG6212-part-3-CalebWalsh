using System.Text.Json.Serialization;

namespace RaceDay.Mvc.Models;

// These classes mirror the JSON that RaceDay.Api returns (see RaceDay.Api/Dtos).
// The JsonPropertyName attributes map the API's names onto the shorter names the views use.

public class AuthResponse
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

public class EventDto
{
    [JsonPropertyName("eventId")]
    public int Id { get; set; }

    [JsonPropertyName("eventName")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("distance")]
    public decimal DistanceKm { get; set; }

    public string EventType { get; set; } = string.Empty;
    public int OrganiserId { get; set; }
    public string? BannerImageUrl { get; set; }
}

public class CategoryDto
{
    [JsonPropertyName("categoryId")]
    public int Id { get; set; }

    [JsonPropertyName("categoryName")]
    public string Name { get; set; } = string.Empty;

    public int? MinAge { get; set; }
    public int? MaxParticipants { get; set; }
    public decimal EntryFee { get; set; }
}

public class EnrolmentDto
{
    [JsonPropertyName("enrolmentId")]
    public int Id { get; set; }

    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool HasResult { get; set; }

    // The API has no status column, so the status is worked out from the event date and whether a result exists.
    [JsonIgnore]
    public string Status =>
        HasResult ? "Completed"
        : EventDate is { } date && date.Date < DateTime.Today ? "Awaiting result"
        : "Confirmed";

    // Filled in by the MVC from GET api/events/{id}/results (not part of the enrolment JSON).
    [JsonIgnore] public TimeSpan? FinishTime { get; set; }
    [JsonIgnore] public int? FinishingPosition { get; set; }
}

public class ResultDto
{
    public int EnrolmentId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan? FinishTime { get; set; }

    [JsonPropertyName("position")]
    public int? FinishingPosition { get; set; }

    public int? TotalFinishers { get; set; }
}

public class ProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int? ClubId { get; set; }
    public string? ProfileImageUrl { get; set; }
}
