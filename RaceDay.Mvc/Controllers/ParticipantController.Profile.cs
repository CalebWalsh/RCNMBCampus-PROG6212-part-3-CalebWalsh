using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public partial class ParticipantController
{
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var result = await Api.GetAsync<ProfileDto>(ApiRoutes.Profile);
        if (SessionExpired(result)) return ToLogin();
        Notify(result);

        var profile = result.Data ?? new ProfileDto();
        HttpContext.Session.SetString("AvatarUrl", profile.ProfileImageUrl ?? string.Empty);
        return View(new ProfileVm
        {
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            ClubId = profile.ClubId,
            Email = profile.Email,
            ProfileImageUrl = profile.ProfileImageUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileVm vm)
    {
        string? imageError = ImageValidator.Validate(vm.Picture);
        if (imageError != null) ModelState.AddModelError(nameof(ProfileVm.Picture), imageError);
        if (!ModelState.IsValid) return View(vm);

        var updated = await Api.PutAsync(ApiRoutes.Profile, new { vm.FullName, vm.PhoneNumber, vm.ClubId });
        if (SessionExpired(updated)) return ToLogin();
        if (!updated.Success)
        {
            ModelState.AddModelError(string.Empty, updated.Error);
            return View(vm);
        }

        HttpContext.Session.SetString("DisplayName", vm.FullName.Trim().Split(' ')[0]);

        if (vm.Picture != null && vm.Picture.Length > 0)
        {
            var upload = await Api.UploadAsync<object>(ApiRoutes.ProfileImage, vm.Picture);
            if (SessionExpired(upload)) return ToLogin();
            if (!upload.Success)
            {
                TempData["Error"] = "Your details were saved, but the picture could not be uploaded. " + upload.Error;
                return RedirectToAction(nameof(Profile));
            }
        }

        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }
}
