using System.ComponentModel.DataAnnotations;
using RaceDay.Mvc.Models.Api;

namespace RaceDay.Mvc.Models.ViewModels;

/// <summary>Shared by the Categories / Enrolments / Results pages: the Organiser's events plus the one being worked on.</summary>
public class EventPickerViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public EventDto? Selected { get; set; }
}

public class CategoriesPageViewModel : EventPickerViewModel
{
    public List<CategoryDto> Categories { get; set; } = new();
    public CategoryFormViewModel NewCategory { get; set; } = new();
}

public class CategoryFormViewModel
{
    public int EventId { get; set; }
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "Please enter a category name."), StringLength(100)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.1, 100000, ErrorMessage = "Distance must be a positive number of kilometres.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }
}

public class EnrolmentsPageViewModel : EventPickerViewModel
{
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}

public class ResultsPageViewModel : EventPickerViewModel
{
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}

public class ResultFormViewModel
{
    public int EventId { get; set; }
    public int EnrolmentId { get; set; }
    public int? ResultId { get; set; }

    [Range(1, 1000000, ErrorMessage = "Finishing position must be 1 or higher.")]
    public int? FinishPosition { get; set; }

    [Display(Name = "Finish time")]
    public TimeOnly? FinishTime { get; set; }
}
