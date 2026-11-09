using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult AccessDenied() => View();

    public IActionResult Error()
    {
        ViewData["Heading"] = "Something went wrong";
        ViewData["Message"] = "RaceDay hit a problem loading that page. Go back and try again.";
        return View("Error");
    }

    public IActionResult Status(int code)
    {
        ViewData["Heading"] = code == 404 ? "Page not found" : "Something went wrong";
        ViewData["Message"] = code == 404
            ? "That page does not exist. Check the address or use the menu."
            : "RaceDay could not load that page. Go back and try again.";
        return View("Error");
    }
}
