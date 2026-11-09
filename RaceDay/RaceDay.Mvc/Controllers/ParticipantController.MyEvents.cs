using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public partial class ParticipantController
{
    public async Task<IActionResult> MyEvents()
    {
        var result = await Api.GetAsync<List<EnrolmentDto>>(ApiRoutes.MyEnrolments);
        if (SessionExpired(result)) return ToLogin();
        Notify(result);
        return View((result.Data ?? new List<EnrolmentDto>()).OrderBy(e => e.EventDate).ToList());
    }
}
