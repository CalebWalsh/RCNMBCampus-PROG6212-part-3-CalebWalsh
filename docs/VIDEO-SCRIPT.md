# Part 3 video script (aim for 10 to 13 minutes)

Record the screen and your voice. Show the running application, not only the code.

1. **Introduce RaceDay (30s).** Say: MVC front end, Part 2 API, SQL Server, Azure Blob Storage, Docker.
   Show the solution in Visual Studio: `RaceDay.Api`, `RaceDay.Mvc`, tests.
2. **Architecture (45s).** Open `Services/RaceDayApiClient.cs`. Explain that MVC only calls the API, keeps the API
   session cookie in the MVC session, and has no DbContext or Azure SDK.
3. **Register and log in (1 min).** Register an Organiser. Log in. Point out the Organiser menu.
   Open `AuthController.Login` and show where UserId and Role are stored in session.
4. **Organiser dashboard (45s).** Show events, upcoming dates and enrolment totals.
5. **Events and banner (1.5 min).** Create an event with a banner. Edit it. Show the banner on the event page.
   Open the API `EventsController.UploadBanner` action and `Services/BlobStorageService.cs`. Show the image in the Azure portal container.
6. **Categories, enrolments, results (1.5 min).** Add categories. Show the enrolments page. Capture a result.
7. **Log out (15s).** Show that a protected URL now redirects to login.
8. **Participant (3 min).** Register and log in as a Participant. Point out the different menu.
   Browse and filter events. Open details. Enter an event. Show My events with the status colour (Confirmed, Awaiting result, Completed).
   Show My results (event, date, category, time, position). Update the profile and upload a picture.
9. **Role protection (30s).** Paste an Organiser URL while logged in as Participant and show the access denied page.
10. **Docker (2.5 min).** Stop the local MVC. Show Docker Desktop running. In a terminal run `docker --version`,
    `docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc:part3 .` (from the `RaceDay` folder), `docker images`, the run command from the README,
    then `docker ps`. Open http://localhost:8080 and log in. Show a data page working through the API.
    Open the Dockerfile and explain base, build, publish and final stages, and why the API address uses
    `host.docker.internal`.
11. **GitHub and CI (45s).** Show the commit history (20+ commits on the Part 3 repo) and the green workflow run.
12. **Close (15s).** Summarise what was built.

Before recording: start SQL Server, run the API on `http://0.0.0.0:5000`, and have two browser profiles
(or one normal and one private window) ready for the two roles.
