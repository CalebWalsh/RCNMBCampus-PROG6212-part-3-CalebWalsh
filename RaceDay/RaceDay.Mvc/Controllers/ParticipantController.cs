using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuth("Participant")]
public partial class ParticipantController : AppController
{
    public ParticipantController(RaceDayApiClient api) : base(api) { }
}
