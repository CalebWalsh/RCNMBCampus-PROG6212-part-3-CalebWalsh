using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

[SessionAuth]
public class EventsController : AppController
{
    public EventsController(RaceDayApiClient api) : base(api) { }

    public async Task<IActionResult> Details(int id)
    {
        var eventResult = await Api.GetAsync<EventDto>(ApiRoutes.Event(id));
        if (SessionExpired(eventResult)) return ToLogin();
        if (!eventResult.Success || eventResult.Data == null)
        {
            TempData["Error"] = string.IsNullOrWhiteSpace(eventResult.Error) ? "That event could not be found." : eventResult.Error;
            return HttpContext.Session.IsRole("Organiser")
                ? RedirectToAction("Events", "Organiser")
                : RedirectToAction("Index", "Participant");
        }

        var vm = new EventDetailsVm
        {
            Event = eventResult.Data,
            IsParticipant = HttpContext.Session.IsRole("Participant")
        };

        var categories = await Api.GetAsync<List<CategoryDto>>(ApiRoutes.Categories(id));
        if (SessionExpired(categories)) return ToLogin();
        vm.Categories = categories.Data ?? new List<CategoryDto>();

        if (vm.IsParticipant)
        {
            var mine = await Api.GetAsync<List<EnrolmentDto>>(ApiRoutes.MyEnrolments);
            if (SessionExpired(mine)) return ToLogin();
            vm.Enrolment = mine.Data?.FirstOrDefault(e => e.EventId == id);
        }
        return View(vm);
    }

    [HttpPost]
    [SessionAuth("Participant")]
    public async Task<IActionResult> Enter(int eventId, int categoryId)
    {
        if (categoryId == 0)
        {
            TempData["Error"] = "Choose a category before entering the event.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        var result = await Api.PostAsync(ApiRoutes.EventEnrolments(eventId), new { CategoryId = categoryId });
        if (SessionExpired(result)) return ToLogin();
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        TempData["Success"] = "You have entered the event.";
        return RedirectToAction("MyEvents", "Participant");
    }
}
