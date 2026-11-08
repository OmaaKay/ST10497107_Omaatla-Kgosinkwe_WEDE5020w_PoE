using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Users;
using RaceDay.Api.Security;
using RaceDay.Api.Services;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[RequireAuthenticated]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IBlobStorageService _blobStorage;

    public UsersController(RaceDayDbContext db, IBlobStorageService blobStorage)
    {
        _db = db;
        _blobStorage = blobStorage;
    }

    /// <summary>Retrieves the authenticated user's own profile.</summary>
    /// <response code="200">Profile returned.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserProfileResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth.HasValue ? DateOnly.FromDateTime(user.DateOfBirth.Value) : null,
            Role = user.Role!.RoleName,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }

    /// <summary>Updates the authenticated user's own profile.</summary>
    /// <response code="200">Profile updated.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return Unauthorized();
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth?.ToDateTime(TimeOnly.MinValue);

        await _db.SaveChangesAsync();

        return Ok(new UserProfileResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth.HasValue ? DateOnly.FromDateTime(user.DateOfBirth.Value) : null,
            Role = user.Role!.RoleName,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }

    /// <summary>Uploads (or replaces) the authenticated user's profile picture. Stored in Azure Blob Storage.</summary>
    [HttpPost("me/picture")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadPicture(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        try
        {
            user.ProfilePictureUrl = await _blobStorage.UploadImageAsync(file, "profiles", cancellationToken);
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
        return Ok(new ImageUploadResponse { Url = user.ProfilePictureUrl });
    }
}
