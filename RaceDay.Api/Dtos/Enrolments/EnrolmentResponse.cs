namespace RaceDay.Api.Dtos.Enrolments;

public class EnrolmentResponse
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int EventCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;

    // Part 3 additions so the MVC pages need no second lookup.
    public DateOnly EventDate { get; set; }
    public string EventLocation { get; set; } = string.Empty;
    public int? ResultId { get; set; }
    public int? FinishPosition { get; set; }
    public TimeOnly? FinishTime { get; set; }
}
