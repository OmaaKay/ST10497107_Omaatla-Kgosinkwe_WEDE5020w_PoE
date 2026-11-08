namespace RaceDay.Api.Dtos.Results;

/// <summary>One row of a Participant's personal race history (GET /api/results/me).</summary>
public class MyResultResponse
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeOnly? FinishTime { get; set; }
    public int? FinishPosition { get; set; }

    /// <summary>How many participants have a recorded finishing position for the same event ("47th out of 312").</summary>
    public int TotalFinishers { get; set; }
}
