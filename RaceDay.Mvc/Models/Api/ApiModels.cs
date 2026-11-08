namespace RaceDay.Mvc.Models.Api;

// Shapes returned by the RaceDay API (Part 2 + Part 3 additions). The MVC app only reads and displays them.

public enum EventType
{
    Run = 0,
    Walk = 1,
    Cycle = 2
}

public class AuthDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ProfileDto
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

public class EventDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public int MaximumParticipants { get; set; }
    public DateOnly RegistrationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public int? OrganiserId { get; set; }
    public int EnrolmentCount { get; set; }
}

public class CategoryDto
{
    public int EventCategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DistanceKm { get; set; }
    public bool IsActive { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int EventCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string EventLocation { get; set; } = string.Empty;
    public int? ResultId { get; set; }
    public int? FinishPosition { get; set; }
    public TimeOnly? FinishTime { get; set; }
}

public class MyResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeOnly? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
    public int TotalFinishers { get; set; }
}

public class ImageUploadDto
{
    public string Url { get; set; } = string.Empty;
}
