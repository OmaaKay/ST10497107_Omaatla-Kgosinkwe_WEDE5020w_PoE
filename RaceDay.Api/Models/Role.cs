using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// A user role: "Participant" or "Organiser". Matches the Part 1 ERD's Role
/// entity. Seeded once at startup — never created via the API.
/// </summary>
public class Role
{
    public int RoleId { get; set; }

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}

/// <summary>
/// Well-known role names. Used instead of magic strings when checking or
/// assigning roles, and in [RequireRole] attributes on controllers.
/// </summary>
public static class RoleNames
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}
