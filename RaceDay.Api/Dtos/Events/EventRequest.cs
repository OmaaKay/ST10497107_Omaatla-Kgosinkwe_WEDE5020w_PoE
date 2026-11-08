using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos.Events;

/// <summary>Body for POST/PUT /api/events.</summary>
public class EventRequest
{
    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateOnly EventDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal? DistanceKm { get; set; }

    [Required]
    public EventType EventType { get; set; }

    [Range(1, int.MaxValue)]
    public int MaximumParticipants { get; set; }

    [Required]
    public DateOnly RegistrationDeadline { get; set; }
}
