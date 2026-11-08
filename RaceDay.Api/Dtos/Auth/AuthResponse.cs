namespace RaceDay.Api.Dtos.Auth;

/// <summary>Returned by register/login on success. The session cookie carries the actual auth state.</summary>
public class AuthResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
