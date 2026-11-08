using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.Enrolments;

/// <summary>Body for POST /api/events/{eventId}/enrolments — the participant selects a category.</summary>
public class EnrolmentRequest
{
    [Required]
    public int EventCategoryId { get; set; }
}
