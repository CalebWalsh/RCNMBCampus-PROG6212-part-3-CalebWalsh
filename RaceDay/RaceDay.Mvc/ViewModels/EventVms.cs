using System.ComponentModel.DataAnnotations;
using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.ViewModels;

public class EventFormVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter the event name.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a description.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose the event date."), DataType(DataType.Date), Display(Name = "Date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Enter the location.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the distance."), Range(0.1, 100000, ErrorMessage = "Distance must be greater than 0."), Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; } = 5;

    [Required(ErrorMessage = "Choose an event type."), Display(Name = "Event type")]
    public string EventType { get; set; } = "Running";

    public string? BannerImageUrl { get; set; }

    [Display(Name = "Event banner")]
    public IFormFile? Banner { get; set; }
}

public class EventDetailsVm
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public bool IsParticipant { get; set; }
    public EnrolmentDto? Enrolment { get; set; }
}
