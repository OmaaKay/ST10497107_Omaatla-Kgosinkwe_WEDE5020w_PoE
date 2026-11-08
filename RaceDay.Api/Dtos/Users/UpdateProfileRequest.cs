using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.Users;

/// <summary>Body for PUT /api/users/me.</summary>
public class UpdateProfileRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}
