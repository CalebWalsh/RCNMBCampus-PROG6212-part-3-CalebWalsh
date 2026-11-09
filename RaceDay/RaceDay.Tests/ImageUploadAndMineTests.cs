using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Auth;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Tests;

public class FakeImageStorage : IImageStorage
{
    public string? LastFolder { get; private set; }

    public Task<string> UploadAsync(IFormFile file, string folder)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("No file was supplied.");
        if (!file.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only JPG, PNG, GIF and WEBP images are allowed.");

        LastFolder = folder;
        return Task.FromResult($"https://example.test/{folder}/{file.FileName}");
    }
}

public class ImageUploadAndMineTests
{
    private static IFormFile Image(string fileName = "pic.png", int size = 10) =>
        new FormFile(new MemoryStream(new byte[size]), 0, size, "file", fileName);

    [Fact]
    public async Task GetMine_ReturnsOnlyTheOrganisersOwnEvents_IncludingPastOnes()
    {
        using var db = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(db, Roles.Organiser);
        var other = TestHelpers.AddUser(db, Roles.Organiser);
        TestHelpers.AddEvent(db, organiser.UserID, DateTime.Today.AddDays(10));
        TestHelpers.AddEvent(db, organiser.UserID, DateTime.Today.AddDays(-10));
        TestHelpers.AddEvent(db, other.UserID, DateTime.Today.AddDays(5));
        var controller = new EventsController(db).WithSession(organiser.UserID, Roles.Organiser);

        var result = await controller.GetMine();

        var ok = Assert.IsType<OkObjectResult>(result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventResponse>>(ok.Value).ToList();
        Assert.Equal(2, events.Count);
        Assert.All(events, e => Assert.Equal(organiser.UserID, e.OrganiserId));
    }

    [Fact]
    public async Task UploadBanner_SavesTheImageUrlOnTheEvent()
    {
        using var db = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(db, Roles.Organiser);
        var ev = TestHelpers.AddEvent(db, organiser.UserID);
        var storage = new FakeImageStorage();
        var controller = new EventsController(db).WithSession(organiser.UserID, Roles.Organiser);

        var result = await controller.UploadBanner(ev.EventID, Image("banner.png"), storage);

        Assert.Equal(200, TestHelpers.Status(result));
        Assert.Equal("event-banners", storage.LastFolder);
        Assert.Equal("https://example.test/event-banners/banner.png", db.Events.Single().BannerImageUrl);
    }

    [Fact]
    public async Task UploadBanner_ByAnotherOrganiser_Returns403()
    {
        using var db = TestHelpers.NewContext();
        var owner = TestHelpers.AddUser(db, Roles.Organiser);
        var other = TestHelpers.AddUser(db, Roles.Organiser);
        var ev = TestHelpers.AddEvent(db, owner.UserID);
        var controller = new EventsController(db).WithSession(other.UserID, Roles.Organiser);

        var result = await controller.UploadBanner(ev.EventID, Image(), new FakeImageStorage());

        Assert.Equal(403, TestHelpers.Status(result));
        Assert.Null(db.Events.Single().BannerImageUrl);
    }

    [Fact]
    public async Task UploadBanner_MissingEvent_Returns404()
    {
        using var db = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(db, Roles.Organiser);
        var controller = new EventsController(db).WithSession(organiser.UserID, Roles.Organiser);

        var result = await controller.UploadBanner(99, Image(), new FakeImageStorage());

        Assert.Equal(404, TestHelpers.Status(result));
    }

    [Fact]
    public async Task UploadBanner_InvalidImage_Returns400()
    {
        using var db = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(db, Roles.Organiser);
        var ev = TestHelpers.AddEvent(db, organiser.UserID);
        var controller = new EventsController(db).WithSession(organiser.UserID, Roles.Organiser);

        var result = await controller.UploadBanner(ev.EventID, Image("virus.exe"), new FakeImageStorage());

        Assert.Equal(400, TestHelpers.Status(result));
        Assert.Null(db.Events.Single().BannerImageUrl);
    }

    [Fact]
    public async Task UploadImage_SavesTheProfilePictureUrl()
    {
        using var db = TestHelpers.NewContext();
        var user = TestHelpers.AddUser(db, Roles.Participant);
        var storage = new FakeImageStorage();
        var controller = new UsersController(db).WithSession(user.UserID, user.Role);

        var result = await controller.UploadImage(Image("me.png"), storage);

        Assert.Equal(200, TestHelpers.Status(result));
        Assert.Equal("profile-pictures", storage.LastFolder);
        Assert.Equal("https://example.test/profile-pictures/me.png", db.Users.Single().ProfileImageUrl);
    }

    [Fact]
    public async Task GetMe_IncludesTheProfileImageUrl()
    {
        using var db = TestHelpers.NewContext();
        var user = TestHelpers.AddUser(db, Roles.Participant);
        user.ProfileImageUrl = "https://example.test/me.png";
        db.SaveChanges();
        var controller = new UsersController(db).WithSession(user.UserID, user.Role);

        var result = await controller.GetMe();

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<UserResponse>(ok.Value);
        Assert.Equal("https://example.test/me.png", body.ProfileImageUrl);
    }

    [Fact]
    public async Task MyResults_IncludesCategoryEventDateAndTotalFinishers()
    {
        using var db = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(db, Roles.Organiser);
        var ev = TestHelpers.AddEvent(db, organiser.UserID, DateTime.Today.AddDays(-7));
        var category = TestHelpers.AddCategory(db, ev.EventID, "21km");

        var runners = new List<User>();
        for (int i = 1; i <= 3; i++)
        {
            var runner = TestHelpers.AddUser(db, Roles.Participant, $"Runner {i}");
            var enrolment = TestHelpers.AddEnrolment(db, runner.UserID, ev.EventID, category.CategoryID);
            db.Results.Add(new Result
            {
                EnrolmentID = enrolment.EnrolmentID,
                FinishTime = TimeSpan.FromMinutes(100 + i),
                Position = i,
                CapturedDate = DateTime.Now
            });
            runners.Add(runner);
        }
        db.SaveChanges();

        var controller = new ResultsController(db).WithSession(runners[1].UserID, Roles.Participant);

        var result = await controller.GetMine();

        var ok = Assert.IsType<OkObjectResult>(result);
        var mine = Assert.IsAssignableFrom<IEnumerable<ResultResponse>>(ok.Value).Single();
        Assert.Equal(2, mine.Position);
        Assert.Equal(3, mine.TotalFinishers);
        Assert.Equal("21km", mine.CategoryName);
        Assert.Equal(ev.EventDate, mine.EventDate);
    }

    [Fact]
    public async Task MyEnrolments_ReportsEventDateAndWhetherAResultExists()
    {
        using var db = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(db, Roles.Organiser);
        var participant = TestHelpers.AddUser(db, Roles.Participant);
        var ev = TestHelpers.AddEvent(db, organiser.UserID, DateTime.Today.AddDays(-3));
        var category = TestHelpers.AddCategory(db, ev.EventID);
        var enrolment = TestHelpers.AddEnrolment(db, participant.UserID, ev.EventID, category.CategoryID);
        db.Results.Add(new Result { EnrolmentID = enrolment.EnrolmentID, FinishTime = TimeSpan.FromMinutes(50), Position = 1, CapturedDate = DateTime.Now });
        db.SaveChanges();
        var controller = new EnrolmentsController(db).WithSession(participant.UserID, Roles.Participant);

        var result = await controller.GetMine();

        var ok = Assert.IsType<OkObjectResult>(result);
        var row = Assert.IsAssignableFrom<IEnumerable<EnrolmentResponse>>(ok.Value).Single();
        Assert.True(row.HasResult);
        Assert.Equal(ev.EventDate, row.EventDate);
    }
}
