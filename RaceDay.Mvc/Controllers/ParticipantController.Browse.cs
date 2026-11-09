using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public partial class ParticipantController
{
    public async Task<IActionResult> Index(string? type, string? search)
    {
        var result = await Api.GetAsync<List<EventDto>>(ApiRoutes.Events);
        if (SessionExpired(result)) return ToLogin();
        Notify(result);

        var events = (result.Data ?? new List<EventDto>())
            .Where(e => e.EventDate.Date >= DateTime.Today)
            .ToList();

        if (!string.IsNullOrWhiteSpace(type))
            events = events.Where(e => string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            events = events.Where(e =>
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        return View(new ParticipantHomeVm
        {
            Events = events.OrderBy(e => e.EventDate).ToList(),
            Type = type,
            Search = search
        });
    }
}
