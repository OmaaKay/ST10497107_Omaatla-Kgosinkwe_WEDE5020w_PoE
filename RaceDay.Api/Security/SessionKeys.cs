namespace RaceDay.Api.Security;

/// <summary>Keys used to store the authenticated identity in ASP.NET Core Session.</summary>
public static class SessionKeys
{
    public const string UserId = "RaceDay_UserId";
    public const string Role = "RaceDay_Role";
    public const string Email = "RaceDay_Email";
}
