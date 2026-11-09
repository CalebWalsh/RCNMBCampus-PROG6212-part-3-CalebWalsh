using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.ViewModels;

public class OrganiserEventSummaryVm
{
    public EventDto Event { get; set; } = new();
    public int EnrolmentCount { get; set; }
    public bool IsUpcoming => Event.EventDate.Date >= DateTime.Today;
    public int DaysAway => (Event.EventDate.Date - DateTime.Today).Days;
}

public class OrganiserDashboardVm
{
    public List<OrganiserEventSummaryVm> Events { get; set; } = new();
    public int TotalEvents => Events.Count;
    public int UpcomingEvents => Events.Count(e => e.IsUpcoming);
    public int TotalEnrolments => Events.Sum(e => e.EnrolmentCount);
    public OrganiserEventSummaryVm? Next => Events.Where(e => e.IsUpcoming).OrderBy(e => e.Event.EventDate).FirstOrDefault();
}

public class CategoriesVm
{
    public List<EventDto> Events { get; set; } = new();
    public EventDto? Selected { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

public class EnrolmentsVm
{
    public List<EventDto> Events { get; set; } = new();
    public EventDto? Selected { get; set; }
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}

public class ResultsVm
{
    public List<EventDto> Events { get; set; } = new();
    public EventDto? Selected { get; set; }
    public List<EnrolmentDto> Enrolments { get; set; } = new();
    public bool EventConcluded => Selected != null && Selected.EventDate.Date <= DateTime.Today;
}
