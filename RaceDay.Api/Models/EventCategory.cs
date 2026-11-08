using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// An age or distance category an Organiser defines for a specific Event
/// (e.g. "Under 18", "5km", "Male Open"). Reconciled for Part 2 — see
/// docs/part2-consistency-notes.md for why this changed from a Part 1
/// global category shared across events to a per-event category.
/// </summary>
public class EventCategory
{
    public int EventCategoryId { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? DistanceKm { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
