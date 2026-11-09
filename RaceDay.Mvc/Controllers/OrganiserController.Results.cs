using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public partial class OrganiserController
{
    public async Task<IActionResult> Results(int? eventId)
    {
        var (mine, events) = await LoadMyEventsAsync();
        if (SessionExpired(mine)) return ToLogin();
        Notify(mine);

        var vm = new ResultsVm
        {
            Events = events,
            Selected = events.FirstOrDefault(e => e.Id == eventId) ?? events.FirstOrDefault()
        };

        if (vm.Selected != null)
        {
            var enrolments = await Api.GetAsync<List<EnrolmentDto>>(ApiRoutes.EventEnrolments(vm.Selected.Id));
            if (SessionExpired(enrolments)) return ToLogin();
            Notify(enrolments);
            vm.Enrolments = enrolments.Data ?? new List<EnrolmentDto>();

            // Show results that were already captured, so they are not entered twice.
            var captured = await Api.GetAsync<List<ResultDto>>(ApiRoutes.EventResults(vm.Selected.Id));
            if (SessionExpired(captured)) return ToLogin();
            foreach (var result in captured.Data ?? new List<ResultDto>())
            {
                var match = vm.Enrolments.FirstOrDefault(e => e.Id == result.EnrolmentId);
                if (match == null) continue;
                match.HasResult = true;
                match.FinishTime = result.FinishTime;
                match.FinishingPosition = result.FinishingPosition;
            }
        }
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, string? finishTime, int finishingPosition)
    {
        if (!TimeSpan.TryParse(finishTime, out var time) || time <= TimeSpan.Zero)
        {
            TempData["Error"] = "Enter the finish time as hours:minutes:seconds, for example 00:45:12.";
            return RedirectToAction(nameof(Results), new { eventId });
        }
        if (finishingPosition < 1)
        {
            TempData["Error"] = "Enter a finishing position of 1 or higher.";
            return RedirectToAction(nameof(Results), new { eventId });
        }

        var result = await Api.PostAsync(ApiRoutes.Result(enrolmentId), new
        {
            FinishTime = time,
            Position = finishingPosition
        });
        if (SessionExpired(result)) return ToLogin();
        if (result.Success) TempData["Success"] = "Result saved.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Results), new { eventId });
    }
}
