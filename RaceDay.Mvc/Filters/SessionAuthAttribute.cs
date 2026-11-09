using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Filters;

public class SessionAuthAttribute : ActionFilterAttribute
{
    private readonly string? _role;

    public SessionAuthAttribute(string? role = null) => _role = role;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.IsLoggedIn())
        {
            if (context.Controller is Controller controller)
                controller.TempData["Error"] = "Please log in to continue.";
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        if (_role != null && !session.IsRole(_role))
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
    }
}
