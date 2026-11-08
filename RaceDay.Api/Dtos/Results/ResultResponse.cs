namespace RaceDay.Api.Dtos.Results;

public class ResultResponse
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int? FinishPosition { get; set; }
    public TimeOnly? FinishTime { get; set; }
    public DateTime RecordedAt { get; set; }
}
