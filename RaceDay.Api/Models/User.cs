using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// A registered RaceDay account. Role name has been aligned to the Part 2
/// brief: "Organiser" replaces the Part 1 working name "Administrator".
/// </summary>
public class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed with Microsoft.AspNetCore.Identity.PasswordHasher — never plaintext.</summary>
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    /// <summary>Part 3: public Azure Blob URL of the profile picture (null until uploaded).</summary>
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
