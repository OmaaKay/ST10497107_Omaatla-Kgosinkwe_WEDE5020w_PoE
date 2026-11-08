using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// Resolves the many-to-many User&lt;-&gt;Event relationship, and — per the
/// Part 2 brief — records which EventCategory the Participant selected.
/// A user may enrol in a given event only once (enforced by a unique
/// constraint on UserId+EventId in RaceDayDbContext).
/// </summary>
public class Enrolment
{
    public int EnrolmentId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    /// <summary>The category the Participant selected at enrolment time.</summary>
    public int EventCategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

    public Result? Result { get; set; }
}
