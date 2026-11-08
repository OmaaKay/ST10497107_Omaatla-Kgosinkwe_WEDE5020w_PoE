using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.EventCategories;

/// <summary>Body for POST/PUT /api/events/{eventId}/categories.</summary>
public class EventCategoryRequest
{
    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? DistanceKm { get; set; }
}
