using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuth("Organiser")]
public partial class OrganiserController : AppController
{
    public OrganiserController(RaceDayApiClient api) : base(api) { }

    private async Task<(ApiResult<List<EventDto>> Result, List<EventDto> Events)> LoadMyEventsAsync()
    {
        var result = await Api.GetAsync<List<EventDto>>(ApiRoutes.MyEvents);
        var events = (result.Data ?? new List<EventDto>()).OrderBy(e => e.EventDate).ToList();
        return (result, events);
    }
}
