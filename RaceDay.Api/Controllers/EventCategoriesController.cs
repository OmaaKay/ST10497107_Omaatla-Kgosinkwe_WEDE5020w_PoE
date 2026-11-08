using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.EventCategories;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>
/// Categories belong to a specific event (Part 2 reconciliation — see
/// docs/part2-consistency-notes.md), so these routes are nested under
/// /api/events/{eventId}/categories.
/// </summary>
[ApiController]
[Route("api/events/{eventId}/categories")]
public class EventCategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventCategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Retrieves the categories defined for an event.</summary>
    /// <response code="200">Category list returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(IEnumerable<EventCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var categories = await _db.EventCategories
            .Where(c => c.EventId == eventId)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Creates a category for an event. Organiser only.</summary>
    /// <response code="201">Category created.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="409">A category with this name already exists for this event.</response>
    [HttpPost]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventCategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(int eventId, [FromBody] EventCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var duplicate = await _db.EventCategories
            .AnyAsync(c => c.EventId == eventId && c.CategoryName == request.CategoryName);
        if (duplicate)
        {
            return Conflict(new { message = "A category with this name already exists for this event." });
        }

        var category = new EventCategory
        {
            EventId = eventId,
            CategoryName = request.CategoryName,
            Description = request.Description,
            DistanceKm = request.DistanceKm
        };

        _db.EventCategories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { eventId }, MapToResponse(category));
    }

    /// <summary>Updates a category. Organiser only.</summary>
    /// <response code="200">Category updated.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Category not found for this event.</response>
    [HttpPut("{categoryId}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int eventId, int categoryId, [FromBody] EventCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await _db.EventCategories.FirstOrDefaultAsync(c => c.EventCategoryId == categoryId && c.EventId == eventId);
        if (category is null)
        {
            return NotFound();
        }

        category.CategoryName = request.CategoryName;
        category.Description = request.Description;
        category.DistanceKm = request.DistanceKm;

        await _db.SaveChangesAsync();

        return Ok(MapToResponse(category));
    }

    /// <summary>Deactivates a category. Organiser only.</summary>
    /// <response code="204">Category deactivated.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Category not found for this event.</response>
    [HttpDelete("{categoryId}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int eventId, int categoryId)
    {
        var category = await _db.EventCategories.FirstOrDefaultAsync(c => c.EventCategoryId == categoryId && c.EventId == eventId);
        if (category is null)
        {
            return NotFound();
        }

        category.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static readonly System.Linq.Expressions.Expression<Func<EventCategory, EventCategoryResponse>> ToResponse = c => new EventCategoryResponse
    {
        EventCategoryId = c.EventCategoryId,
        EventId = c.EventId,
        CategoryName = c.CategoryName,
        Description = c.Description,
        DistanceKm = c.DistanceKm,
        IsActive = c.IsActive
    };

    private static EventCategoryResponse MapToResponse(EventCategory c) => new()
    {
        EventCategoryId = c.EventCategoryId,
        EventId = c.EventId,
        CategoryName = c.CategoryName,
        Description = c.Description,
        DistanceKm = c.DistanceKm,
        IsActive = c.IsActive
    };
}
