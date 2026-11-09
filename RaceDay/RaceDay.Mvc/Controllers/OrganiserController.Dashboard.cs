using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public partial class OrganiserController
{
    public async Task<IActionResult> Index()
    {
        var (mine, events) = await LoadMyEventsAsync();
        if (SessionExpired(mine)) return ToLogin();
        Notify(mine);

        var vm = new OrganiserDashboardVm();
        foreach (var item in events)
        {
            var enrolments = await Api.GetAsync<List<EnrolmentDto>>(ApiRoutes.EventEnrolments(item.Id));
            if (SessionExpired(enrolments)) return ToLogin();
            vm.Events.Add(new OrganiserEventSummaryVm
            {
                Event = item,
                EnrolmentCount = enrolments.Data?.Count ?? 0
            });
        }
        return View(vm);
    }
}
