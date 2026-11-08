namespace RaceDay.Api.Dtos.EventCategories;

public class EventCategoryResponse
{
    public int EventCategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DistanceKm { get; set; }
    public bool IsActive { get; set; }
}
