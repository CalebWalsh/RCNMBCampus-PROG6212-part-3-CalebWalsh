using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public partial class ParticipantController
{
    public async Task<IActionResult> MyResults()
    {
        var result = await Api.GetAsync<List<ResultDto>>(ApiRoutes.MyResults);
        if (SessionExpired(result)) return ToLogin();
        Notify(result);
        return View((result.Data ?? new List<ResultDto>()).OrderByDescending(r => r.EventDate).ToList());
    }
}
