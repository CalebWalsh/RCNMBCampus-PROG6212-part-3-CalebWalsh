using System.ComponentModel.DataAnnotations;
using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.ViewModels;

public class ParticipantHomeVm
{
    public List<EventDto> Events { get; set; } = new();
    public string? Type { get; set; }
    public string? Search { get; set; }
}

public class ProfileVm
{
    [Required(ErrorMessage = "Enter your full name."), StringLength(100), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone number can be at most 20 characters."), Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    // Kept so saving the profile does not clear the club the user already has.
    public int? ClubId { get; set; }

    public string? Email { get; set; }
    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Profile picture")]
    public IFormFile? Picture { get; set; }
}
