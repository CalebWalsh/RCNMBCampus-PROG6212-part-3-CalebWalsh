using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public abstract class AppController : Controller
{
    protected AppController(RaceDayApiClient api) => Api = api;

    protected RaceDayApiClient Api { get; }

    protected bool SessionExpired(ApiResult result)
    {
        if (!result.Unauthorized) return false;
        HttpContext.Session.Clear();
        TempData["Error"] = "Your session has expired. Please log in again.";
        return true;
    }

    protected IActionResult ToLogin() => RedirectToAction("Login", "Auth");

    protected void Notify(ApiResult result)
    {
        if (!result.Success) TempData["Error"] = result.Error;
    }

    protected IActionResult? RedirectIfLoggedIn()
    {
        if (!HttpContext.Session.IsLoggedIn()) return null;
        return HttpContext.Session.IsRole("Organiser")
            ? RedirectToAction("Index", "Organiser")
            : RedirectToAction("Index", "Participant");
    }
}
