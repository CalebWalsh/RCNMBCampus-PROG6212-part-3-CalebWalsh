using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public partial class OrganiserController
{
    public async Task<IActionResult> Enrolments(int? eventId)
    {
        var (mine, events) = await LoadMyEventsAsync();
        if (SessionExpired(mine)) return ToLogin();
        Notify(mine);

        var vm = new EnrolmentsVm
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
        }
        return View(vm);
    }
}
