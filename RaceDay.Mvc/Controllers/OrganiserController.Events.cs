using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public partial class OrganiserController
{
    public async Task<IActionResult> Events()
    {
        var (mine, events) = await LoadMyEventsAsync();
        if (SessionExpired(mine)) return ToLogin();
        Notify(mine);
        return View(events);
    }

    [HttpGet]
    public IActionResult CreateEvent() => View("EventForm", new EventFormVm());

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventFormVm vm)
    {
        ValidateBanner(vm);
        if (!ModelState.IsValid) return View("EventForm", vm);

        var created = await Api.PostAsync<EventDto>(ApiRoutes.Events, ToRequest(vm));
        if (SessionExpired(created)) return ToLogin();
        if (!created.Success)
        {
            ModelState.AddModelError(string.Empty, created.Error);
            return View("EventForm", vm);
        }

        int eventId = created.Data?.Id ?? 0;
        if (eventId == 0) eventId = await FindCreatedEventIdAsync(vm);

        string? warning = await UploadBannerAsync(eventId, vm.Banner);
        TempData["Success"] = "Event created.";
        if (warning != null) TempData["Error"] = warning;
        return RedirectToAction(nameof(Events));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var result = await Api.GetAsync<EventDto>(ApiRoutes.Event(id));
        if (SessionExpired(result)) return ToLogin();
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = string.IsNullOrWhiteSpace(result.Error) ? "That event could not be found." : result.Error;
            return RedirectToAction(nameof(Events));
        }

        var e = result.Data;
        return View("EventForm", new EventFormVm
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description ?? string.Empty,
            EventDate = e.EventDate,
            Location = e.Location,
            DistanceKm = e.DistanceKm,
            EventType = e.EventType,
            BannerImageUrl = e.BannerImageUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditEvent(EventFormVm vm)
    {
        ValidateBanner(vm);
        if (!ModelState.IsValid) return View("EventForm", vm);

        var updated = await Api.PutAsync(ApiRoutes.Event(vm.Id), ToRequest(vm));
        if (SessionExpired(updated)) return ToLogin();
        if (!updated.Success)
        {
            ModelState.AddModelError(string.Empty, updated.Error);
            return View("EventForm", vm);
        }

        string? warning = await UploadBannerAsync(vm.Id, vm.Banner);
        TempData["Success"] = "Event updated.";
        if (warning != null) TempData["Error"] = warning;
        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await Api.DeleteAsync(ApiRoutes.Event(id));
        if (SessionExpired(result)) return ToLogin();
        if (result.Success) TempData["Success"] = "Event deleted.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Events));
    }

    private void ValidateBanner(EventFormVm vm)
    {
        string? error = ImageValidator.Validate(vm.Banner);
        if (error != null) ModelState.AddModelError(nameof(EventFormVm.Banner), error);
    }

    private static object ToRequest(EventFormVm vm) => new
    {
        EventName = vm.Name,
        vm.Description,
        vm.EventDate,
        vm.Location,
        Distance = vm.DistanceKm,
        vm.EventType
    };

    private async Task<int> FindCreatedEventIdAsync(EventFormVm vm)
    {
        var (_, events) = await LoadMyEventsAsync();
        return events
            .Where(e => e.Name == vm.Name && e.EventDate.Date == vm.EventDate.Date)
            .OrderByDescending(e => e.Id)
            .Select(e => e.Id)
            .FirstOrDefault();
    }

    private async Task<string?> UploadBannerAsync(int eventId, IFormFile? banner)
    {
        if (banner == null || banner.Length == 0) return null;
        if (eventId == 0) return "The event was saved, but the banner could not be uploaded.";

        var upload = await Api.UploadAsync<object>(ApiRoutes.EventBanner(eventId), banner);
        return upload.Success ? null : "The event was saved, but the banner could not be uploaded. " + upload.Error;
    }
}
