using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos.Events;

public class EventResponse
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public int MaximumParticipants { get; set; }
    public DateOnly RegistrationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public int? OrganiserId { get; set; }

    /// <summary>Number of non-cancelled enrolments (used by the Organiser dashboard).</summary>
    public int EnrolmentCount { get; set; }
}
