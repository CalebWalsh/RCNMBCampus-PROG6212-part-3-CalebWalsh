using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public partial class OrganiserController
{
    public async Task<IActionResult> Categories(int? eventId)
    {
        var (mine, events) = await LoadMyEventsAsync();
        if (SessionExpired(mine)) return ToLogin();
        Notify(mine);

        var vm = new CategoriesVm
        {
            Events = events,
            Selected = events.FirstOrDefault(e => e.Id == eventId) ?? events.FirstOrDefault()
        };

        if (vm.Selected != null)
        {
            var categories = await Api.GetAsync<List<CategoryDto>>(ApiRoutes.Categories(vm.Selected.Id));
            if (SessionExpired(categories)) return ToLogin();
            Notify(categories);
            vm.Categories = categories.Data ?? new List<CategoryDto>();
        }
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int eventId, string? name, decimal entryFee, int? maxParticipants, int? minAge)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Enter a category name, for example 10 km or Under 20.";
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        var result = await Api.PostAsync(ApiRoutes.Categories(eventId), new
        {
            CategoryName = name.Trim(),
            EntryFee = entryFee,
            MaxParticipants = maxParticipants,
            MinAge = minAge
        });
        if (SessionExpired(result)) return ToLogin();
        if (result.Success) TempData["Success"] = "Category added.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
    {
        var result = await Api.DeleteAsync(ApiRoutes.Category(categoryId));
        if (SessionExpired(result)) return ToLogin();
        if (result.Success) TempData["Success"] = "Category deleted.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Categories), new { eventId });
    }
}
