using System.ComponentModel.DataAnnotations;
using RaceDay.Mvc.Models.Api;

namespace RaceDay.Mvc.Models.ViewModels;

public class EventListViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public int TotalUpcoming { get; set; }

    // filters
    public string? Q { get; set; }
    public EventType? Type { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    public bool HasFilters => !string.IsNullOrWhiteSpace(Q) || Type is not null || From is not null || To is not null;
}

public class EventDetailsViewModel
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>The signed-in Participant's own enrolment for this event, if any.</summary>
    public EnrolmentDto? MyEnrolment { get; set; }

    public bool IsParticipant { get; set; }
}

public class EventFormViewModel
{
    public int? EventId { get; set; }

    [Required(ErrorMessage = "Please enter the event name."), StringLength(150)]
    [Display(Name = "Event name")]
    public string EventName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose the event date.")]
    [Display(Name = "Event date")]
    public DateOnly? EventDate { get; set; }

    [Required(ErrorMessage = "Please enter the start time.")]
    [Display(Name = "Start time")]
    public TimeOnly? StartTime { get; set; }

    [Required(ErrorMessage = "Please enter the location."), StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 100000, ErrorMessage = "Distance must be a positive number of kilometres.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }

    [Required(ErrorMessage = "Please choose an event type.")]
    [Display(Name = "Event type")]
    public EventType? EventType { get; set; }

    [Required(ErrorMessage = "Please enter the maximum number of participants.")]
    [Range(1, 1000000, ErrorMessage = "Maximum participants must be at least 1.")]
    [Display(Name = "Maximum participants")]
    public int? MaximumParticipants { get; set; }

    [Required(ErrorMessage = "Please choose the registration deadline.")]
    [Display(Name = "Registration deadline")]
    public DateOnly? RegistrationDeadline { get; set; }

    [Display(Name = "Event banner")]
    public IFormFile? Banner { get; set; }

    public string? ExistingBannerUrl { get; set; }
}

public class DashboardViewModel
{
    public string OrganiserName { get; set; } = string.Empty;
    public List<EventDto> Events { get; set; } = new();
    public int TotalEvents => Events.Count;
    public int TotalEnrolments => Events.Sum(e => e.EnrolmentCount);
    public List<EventDto> Upcoming => Events
        .Where(e => e.EventDate >= DateOnly.FromDateTime(DateTime.Today))
        .OrderBy(e => e.EventDate)
        .ToList();
    public EventDto? NextEvent => Upcoming.FirstOrDefault();
}
