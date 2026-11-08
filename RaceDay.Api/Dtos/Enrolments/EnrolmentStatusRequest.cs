using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.Enrolments;

/// <summary>Body for PUT /api/enrolments/{id}/status (Organiser only).</summary>
public class EnrolmentStatusRequest
{
    /// <summary>Pending, Confirmed or Cancelled.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
