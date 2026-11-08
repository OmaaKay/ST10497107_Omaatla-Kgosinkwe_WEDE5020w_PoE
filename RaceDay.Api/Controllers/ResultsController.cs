using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Results;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves results. Organisers see all results; Participants see only
    /// their own (enforced here, not just by role, since Result has no
    /// direct owner column).
    /// </summary>
    /// <response code="200">Result list returned.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet("api/results")]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(IEnumerable<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var role = HttpContext.Session.GetRole();
        var query = _db.Results.AsQueryable();

        if (role == RoleNames.Participant)
        {
            var userId = HttpContext.Session.GetUserId()!.Value;
            query = query.Where(r => r.Enrolment!.UserId == userId);
        }

        var results = await query.Select(ToResponse).ToListAsync();
        return Ok(results);
    }

    /// <summary>Retrieves one result. Participants may only view their own.</summary>
    /// <response code="200">Result returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Participant attempting to view another participant's result.</response>
    /// <response code="404">Result not found.</response>
    /// <summary>A Participant's personal race history across all events they have a recorded result for.</summary>
    [HttpGet("api/results/me")]
    [RequireRole(RoleNames.Participant)]
    [ProducesResponseType(typeof(IEnumerable<MyResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        var results = await _db.Results
            .Where(r => r.Enrolment!.UserId == userId)
            .OrderByDescending(r => r.Enrolment!.Event!.EventDate)
            .Select(r => new MyResultResponse
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.Event!.EventName,
                EventDate = r.Enrolment.Event.EventDate,
                CategoryName = r.Enrolment.EventCategory!.CategoryName,
                FinishTime = r.FinishTime,
                FinishPosition = r.FinishPosition,
                TotalFinishers = _db.Results.Count(x =>
                    x.Enrolment!.EventId == r.Enrolment.EventId && x.FinishPosition != null)
            })
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("api/results/{id}")]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _db.Results.Include(r => r.Enrolment).FirstOrDefaultAsync(r => r.ResultId == id);
        if (result is null)
        {
            return NotFound();
        }

        var role = HttpContext.Session.GetRole();
        if (role == RoleNames.Participant)
        {
            var userId = HttpContext.Session.GetUserId()!.Value;
            if (result.Enrolment!.UserId != userId)
            {
                return Forbid();
            }
        }

        return Ok(MapToResponse(result));
    }

    /// <summary>Records a result for an enrolment. Organiser only.</summary>
    /// <response code="201">Result recorded.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Enrolment not found.</response>
    /// <response code="409">A result already exists for this enrolment.</response>
    [HttpPost("api/enrolments/{enrolmentId}/result")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Record(int enrolmentId, [FromBody] ResultRequest request)
    {
        var enrolment = await _db.Enrolments.Include(e => e.Result).FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (enrolment.Result is not null)
        {
            return Conflict(new { message = "A result has already been recorded for this enrolment." });
        }

        var result = new Result
        {
            EnrolmentId = enrolmentId,
            FinishPosition = request.FinishPosition,
            FinishTime = request.FinishTime
        };

        _db.Results.Add(result);
        enrolment.Status = "Completed";
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = result.ResultId }, MapToResponse(result));
    }

    /// <summary>Updates an existing result. Organiser only.</summary>
    /// <response code="200">Result updated.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Result not found.</response>
    [HttpPut("api/results/{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ResultRequest request)
    {
        var result = await _db.Results.FindAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        result.FinishPosition = request.FinishPosition;
        result.FinishTime = request.FinishTime;

        await _db.SaveChangesAsync();

        return Ok(MapToResponse(result));
    }

    /// <summary>Deletes an incorrectly recorded result. Organiser only.</summary>
    /// <response code="204">Result deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Result not found.</response>
    [HttpDelete("api/results/{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.Results.Include(r => r.Enrolment).FirstOrDefaultAsync(r => r.ResultId == id);
        if (result is null)
        {
            return NotFound();
        }

        if (result.Enrolment is not null && result.Enrolment.Status == "Completed")
        {
            result.Enrolment.Status = "Confirmed";
        }

        _db.Results.Remove(result);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static ResultResponse MapToResponse(Result r) => new()
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        FinishPosition = r.FinishPosition,
        FinishTime = r.FinishTime,
        RecordedAt = r.RecordedAt
    };

    private static readonly System.Linq.Expressions.Expression<Func<Result, ResultResponse>> ToResponse = r => new ResultResponse
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        FinishPosition = r.FinishPosition,
        FinishTime = r.FinishTime,
        RecordedAt = r.RecordedAt
    };
}
