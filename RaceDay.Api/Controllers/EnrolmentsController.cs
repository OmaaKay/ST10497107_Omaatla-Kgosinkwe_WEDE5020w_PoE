using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Enrolments;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Enrols the authenticated Participant in an event, selecting a category.</summary>
    /// <response code="201">Enrolment created.</response>
    /// <response code="400">Missing/invalid category, or category belongs to a different event.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Organiser, not Participant.</response>
    /// <response code="404">Event or category not found.</response>
    /// <response code="409">Already enrolled in this event.</response>
    [HttpPost("api/events/{eventId}/enrolments")]
    [RequireRole(RoleNames.Participant)]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        var category = await _db.EventCategories
            .FirstOrDefaultAsync(c => c.EventCategoryId == request.EventCategoryId && c.EventId == eventId);
        if (category is null)
        {
            return BadRequest(new { message = "The selected category does not belong to this event." });
        }

        var userId = HttpContext.Session.GetUserId()!.Value;

        var alreadyEnrolled = await _db.Enrolments.AnyAsync(e => e.UserId == userId && e.EventId == eventId);
        if (alreadyEnrolled)
        {
            return Conflict(new { message = "You are already enrolled in this event." });
        }

        var enrolment = new Enrolment
        {
            UserId = userId,
            EventId = eventId,
            EventCategoryId = category.EventCategoryId
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var response = await BuildResponse(enrolment.EnrolmentId);
        return CreatedAtAction(nameof(GetMine), null, response);
    }

    /// <summary>Retrieves the authenticated Participant's own enrolments.</summary>
    /// <response code="200">Enrolment list returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Organiser, not Participant.</response>
    [HttpGet("api/enrolments/me")]
    [RequireRole(RoleNames.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        var enrolments = await _db.Enrolments
            .Where(e => e.UserId == userId)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Cancels the authenticated Participant's own enrolment.</summary>
    /// <response code="204">Enrolment cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Organiser, or trying to cancel someone else's enrolment.</response>
    /// <response code="404">Enrolment not found.</response>
    [HttpDelete("api/enrolments/{id}")]
    [RequireRole(RoleNames.Participant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment is null)
        {
            return NotFound();
        }

        if (enrolment.UserId != userId)
        {
            return Forbid();
        }

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Retrieves all enrolments for an event. Organiser only.</summary>
    /// <response code="200">Enrolment list returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("api/events/{eventId}/enrolments")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
        {
            return NotFound();
        }

        var enrolments = await _db.Enrolments
            .Where(e => e.EventId == eventId)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Organiser confirms (or cancels / resets) an enrolment, e.g. Pending to Confirmed.</summary>
    [HttpPut("api/enrolments/{id}/status")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] EnrolmentStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var allowed = new[] { "Pending", "Confirmed", "Cancelled" };
        var status = allowed.FirstOrDefault(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        if (status is null)
        {
            return BadRequest(new { message = "Status must be Pending, Confirmed or Cancelled." });
        }

        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(e => e.EnrolmentId == id);
        if (enrolment is null)
        {
            return NotFound();
        }

        enrolment.Status = status;
        await _db.SaveChangesAsync();

        return Ok(await BuildResponse(id));
    }

    private async Task<EnrolmentResponse> BuildResponse(int enrolmentId)
    {
        return await _db.Enrolments.Where(e => e.EnrolmentId == enrolmentId).Select(ToResponse).FirstAsync();
    }

    private static readonly System.Linq.Expressions.Expression<Func<Enrolment, EnrolmentResponse>> ToResponse = e => new EnrolmentResponse
    {
        EnrolmentId = e.EnrolmentId,
        UserId = e.UserId,
        ParticipantName = e.User!.FirstName + " " + e.User.LastName,
        EventId = e.EventId,
        EventName = e.Event!.EventName,
        EventCategoryId = e.EventCategoryId,
        CategoryName = e.EventCategory!.CategoryName,
        EnrolmentDate = e.EnrolmentDate,
        Status = e.Status,
        EventDate = e.Event.EventDate,
        EventLocation = e.Event.Location,
        ResultId = e.Result != null ? e.Result.ResultId : (int?)null,
        FinishPosition = e.Result != null ? e.Result.FinishPosition : null,
        FinishTime = e.Result != null ? e.Result.FinishTime : null
    };
}
