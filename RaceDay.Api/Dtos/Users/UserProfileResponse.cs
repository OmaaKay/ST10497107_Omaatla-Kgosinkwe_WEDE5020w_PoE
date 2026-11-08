namespace RaceDay.Api.Dtos.Users;

public class UserProfileResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}
