namespace RaceDay.Api.Dtos.Results;

/// <summary>Body for POST /api/enrolments/{id}/result and PUT /api/results/{id}.</summary>
public class ResultRequest
{
    [System.ComponentModel.DataAnnotations.Range(1, 1000000)]
    public int? FinishPosition { get; set; }
    public TimeOnly? FinishTime { get; set; }
}
