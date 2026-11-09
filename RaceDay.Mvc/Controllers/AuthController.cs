using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class AuthController : AppController
{
    public AuthController(RaceDayApiClient api) : base(api) { }

    [HttpGet]
    public IActionResult Register() => RedirectIfLoggedIn() ?? View(new RegisterVm());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await Api.PostAsync(ApiRoutes.Register, new
        {
            vm.FullName,
            vm.Email,
            vm.Password,
            vm.Role
        });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(vm);
        }

        TempData["Success"] = "Registration successful. Log in to continue.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => RedirectIfLoggedIn() ?? View(new LoginVm());

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        HttpContext.Session.Clear();

        var result = await Api.PostAsync<AuthResponse>(ApiRoutes.Login, new { vm.Email, vm.Password });
        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty,
                result.StatusCode is 400 or 401 or 403 or 404
                    ? "Email or password is incorrect."
                    : string.IsNullOrWhiteSpace(result.Error) ? "Login failed. Please try again." : result.Error);
            return View(vm);
        }

        var auth = result.Data;
        HttpContext.Session.SetInt32("UserId", auth.UserId);
        HttpContext.Session.SetString("Role", auth.Role);
        HttpContext.Session.SetString("DisplayName", FirstName(auth.FullName) ?? auth.Email ?? vm.Email);

        return RedirectIfLoggedIn() ?? RedirectToAction("Index", "Home");
    }

    private static string? FirstName(string? fullName) =>
        string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim().Split(' ')[0];

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await Api.PostAsync(ApiRoutes.Logout);
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }
}
