namespace RaceDay.Api.Models;

/// <summary>
/// A finish result for one enrolment. One-to-zero-or-one with Enrolment,
/// enforced by a unique constraint on EnrolmentId.
/// </summary>
public class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }

    public int? FinishPosition { get; set; }

    public TimeOnly? FinishTime { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
