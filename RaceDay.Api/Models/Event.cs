using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// A race event created by an Organiser.
///
/// PART 2 ALIGNMENT: in the Part 1 design, Event belonged to a single global
/// EventCategory (e.g. "5K"). The Part 2 brief instead requires an Organiser
/// to "define age or distance categories FOR EACH EVENT", and for a
/// Participant to select one of those categories when enrolling. Event no
/// longer references a single global category; instead EventCategory now
/// belongs to one Event (see EventCategory.cs), and the participant's choice
/// is recorded on Enrolment. See docs/part2-consistency-notes.md.
/// </summary>
public class Event
{
    public int EventId { get; set; }

    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateOnly EventDate { get; set; }

    public TimeOnly StartTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal? DistanceKm { get; set; }

    public EventType EventType { get; set; }

    public int MaximumParticipants { get; set; }

    public DateOnly RegistrationDeadline { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Open"; // Open, Closed, Completed, Cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Part 3: public Azure Blob URL of the event banner image (null until uploaded).</summary>
    [MaxLength(500)]
    public string? BannerUrl { get; set; }

    /// <summary>Part 3: the Organiser who created the event (used for the Organiser dashboard).</summary>
    public int? OrganiserId { get; set; }

    public ICollection<EventCategory> Categories { get; set; } = new List<EventCategory>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
