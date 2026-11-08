using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using RaceDay.Api.Security;
using RaceDay.Api.Services;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IBlobStorageService _blobStorage;

    public EventsController(RaceDayDbContext db, IBlobStorageService blobStorage)
    {
        _db = db;
        _blobStorage = blobStorage;
    }

    /// <summary>Retrieves all events. Requires login, but is open to both roles.</summary>
    /// <response code="200">Event list returned.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool mine = false)
    {
        var query = _db.Events.AsQueryable();

        // Part 3: the Organiser dashboard asks for only the events that Organiser created.
        if (mine)
        {
            var userId = HttpContext.Session.GetUserId()!.Value;
            query = query.Where(e => e.OrganiserId == userId);
        }

        var events = await query.OrderBy(e => e.EventDate).Select(ToResponse).ToListAsync();
        return Ok(events);
    }

    /// <summary>Retrieves one event by id.</summary>
    /// <response code="200">Event returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("{id}")]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.Where(e => e.EventId == id).Select(ToResponse).FirstOrDefaultAsync();
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>Creates a new race event. Organiser only.</summary>
    /// <response code="201">Event created.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    [HttpPost]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = new Event
        {
            EventName = request.EventName,
            Description = request.Description,
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            Location = request.Location,
            DistanceKm = request.DistanceKm,
            EventType = request.EventType,
            MaximumParticipants = request.MaximumParticipants,
            RegistrationDeadline = request.RegistrationDeadline,
            OrganiserId = HttpContext.Session.GetUserId()
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, MapToResponse(ev));
    }

    /// <summary>Updates a race event. Organiser only.</summary>
    /// <response code="200">Event updated.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    [HttpPut("{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        ev.EventName = request.EventName;
        ev.Description = request.Description;
        ev.EventDate = request.EventDate;
        ev.StartTime = request.StartTime;
        ev.Location = request.Location;
        ev.DistanceKm = request.DistanceKm;
        ev.EventType = request.EventType;
        ev.MaximumParticipants = request.MaximumParticipants;
        ev.RegistrationDeadline = request.RegistrationDeadline;

        await _db.SaveChangesAsync();

        return Ok(MapToResponse(ev));
    }

    /// <summary>Cancels a race event. Organiser only. Historical enrolments/results are kept.</summary>
    /// <response code="204">Event cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    /// <summary>Uploads (or replaces) the banner image for an event. Stored in Azure Blob Storage.</summary>
    [HttpPost("{id}/banner")]
    [RequireRole(RoleNames.Organiser)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var ev = await _db.Events.FindAsync(new object[] { id }, cancellationToken);
        if (ev is null)
        {
            return NotFound();
        }

        try
        {
            ev.BannerUrl = await _blobStorage.UploadImageAsync(file, "banners", cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (Azure.RequestFailedException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "The image could not be stored in Azure Blob Storage. Please try again later." });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new ImageUploadResponse { Url = ev.BannerUrl });
    }

    [HttpDelete("{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        // Soft cancel rather than delete, so historical enrolments/results survive.
        ev.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static readonly System.Linq.Expressions.Expression<Func<Event, EventResponse>> ToResponse = e => new EventResponse
    {
        EventId = e.EventId,
        EventName = e.EventName,
        Description = e.Description,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        MaximumParticipants = e.MaximumParticipants,
        RegistrationDeadline = e.RegistrationDeadline,
        Status = e.Status,
        BannerUrl = e.BannerUrl,
        OrganiserId = e.OrganiserId,
        EnrolmentCount = e.Enrolments.Count(x => x.Status != "Cancelled")
    };

    private static EventResponse MapToResponse(Event e) => new()
    {
        EventId = e.EventId,
        EventName = e.EventName,
        Description = e.Description,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        MaximumParticipants = e.MaximumParticipants,
        RegistrationDeadline = e.RegistrationDeadline,
        Status = e.Status,
        BannerUrl = e.BannerUrl,
        OrganiserId = e.OrganiserId,
        EnrolmentCount = 0
    };
}
