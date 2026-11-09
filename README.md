# RaceDay

RaceDay is an event platform for running, walking and cycling events. Organisers create events, categories and results.
Participants browse events, enter them and see their results.

This repository holds **Part 3**: the ASP.NET Core MVC web app (`RaceDay/RaceDay.Mvc`), together with the Part 2 REST API
(`RaceDay/RaceDay.Api`) and its tests (`RaceDay/RaceDay.Tests`).

```
.github/workflows/ci.yml   build, test and Docker build
docs/                      video script and database script
RaceDay/
  RaceDay.sln
  RaceDay.Api/             Part 2 REST API (SQL Server, EF Core, Azure Blob uploads)
  RaceDay.Mvc/             Part 3 MVC web app
  RaceDay.Tests/           unit and pipeline tests
```

### Database setup

1. Install the .NET 8 SDK and have SQL Server or LocalDB available. Set `ConnectionStrings:RaceDayDb` in `RaceDay/RaceDay.Api/appsettings.json`.
2. From the `RaceDay` folder create the database:
   ```
   dotnet ef migrations add InitialCreate --project RaceDay.Api
   dotnet ef database update --project RaceDay.Api
   ```
   If you already have a RaceDay database from Part 2, run `docs/RaceDay_Part3_ImageColumns.sql` instead. It only adds the two image columns.

## Part 3 - MVC Web Application

Part 3 adds `RaceDay.Mvc`, an ASP.NET Core MVC front end that uses the Part 2 REST API for every data operation.
The MVC project has no database access and no Azure SDK. Images travel from the browser to MVC, then to the API,
then to Azure Blob Storage.

```
Browser -> RaceDay.Mvc -> RaceDay.Api -> SQL Server
                                     \-> Azure Blob Storage
```

### Prerequisites

- .NET 8 SDK
- SQL Server with the RaceDay database (see Part 1 script and Part 2 setup)
- An Azure Storage account (for banner and profile images)
- Docker Desktop (Linux containers, WSL 2)

### Run the API

```
dotnet run --project RaceDay.Api --urls http://0.0.0.0:5000
```

Swagger: http://localhost:5000/swagger

### Run the MVC app locally

The API address is read from `RaceDay.Mvc/appsettings.json` (`ApiSettings:BaseUrl`, default `http://localhost:5000/`).

```
dotnet run --project RaceDay.Mvc --urls http://localhost:5100
```

Open http://localhost:5100

### Azure Blob Storage setup

1. Create a storage account in the Azure portal and copy its connection string.
2. Store the secret outside Git (the `appsettings.json` value is only a placeholder):

```
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "<connection string>" --project RaceDay.Api
dotnet user-secrets set "AzureBlobStorage:ContainerName" "raceday-images" --project RaceDay.Api
```

3. Allow public blob read access on the storage account so uploaded images display in the browser.
4. Add the two image URL columns to the database: run `dotnet ef migrations add AddImageUrls --project RaceDay.Api`
   then `dotnet ef database update --project RaceDay.Api`, or run `docs/RaceDay_Part3_ImageColumns.sql`.

### Docker

Run these commands from the `RaceDay` folder (the one that contains `RaceDay.sln`). Start the API first (see above).

Build the image:

```
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc:part3 .
```

Run the container (one command):

```
docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ raceday-mvc:part3
```

Open http://localhost:8080

Inside the container `localhost` means the container itself, so the API address uses `host.docker.internal`
to reach the API running on the Windows host.

Useful commands:

```
docker ps
docker logs raceday-mvc
docker rm -f raceday-mvc
```

### Authentication and session flow

1. The user registers or logs in on the MVC site. MVC posts the details to `api/auth/register` or `api/auth/login`.
2. On login the API replies with the user's id and role and sets its own session cookie.
3. MVC stores the user id, role and display name in its own session, and keeps the API cookie server-side so every
   later API call stays authenticated. The API cookie is never sent to the browser.
4. Menus are built from the role stored in the MVC session. Protected pages redirect to the login page when there is no
   session, and send the wrong role to an access denied page.
5. Log out calls `api/auth/logout`, clears the MVC session and returns to the home page.

### Organiser features

- Dashboard: events created, upcoming dates and total enrolments per event
- Create, edit and delete events (name, description, date, location, distance, event type, banner image)
- Add and delete categories for each event (name, entry fee, maximum participants, minimum age)
- View enrolments for each event with category and status
- Capture finish times and finishing positions after an event (a captured result is locked, matching the API's 409 rule)

### Participant features

- Browse upcoming events and filter by type, name or location
- View full event details and categories, then enter an event with a category
- My events with colour-coded enrolment status (green Confirmed, amber Awaiting result, dark blue Completed)
- My results with event, date, category, finish time and finishing position
- Profile page to update personal details and upload a profile picture

### CI/CD

`.github/workflows/ci.yml` restores, builds and tests the whole solution (API, tests and MVC) on every push, and
builds the MVC Docker image in a second job.

![GitHub Actions build](docs/ci-green.png)

### Video presentation

[Watch the Part 3 demonstration on YouTube](PASTE_YOUTUBE_LINK_HERE)


### API additions for Part 3

The MVC app calls the Part 2 API as it was built. Part 3 added only what the new features needed:

| Addition | Purpose |
|---|---|
| `GET api/events/mine` | Organiser's own events, including past ones (needed to capture results) |
| `POST api/events/{id}/banner` | Upload an event banner to Azure Blob Storage (owner only) |
| `POST api/users/me/image` | Upload a profile picture to Azure Blob Storage |
| `BannerImageUrl` on events, `ProfileImageUrl` on users | Stores the blob URL |
| `EventDate` and `HasResult` on enrolments | Lets My Events show a status |
| `EventDate`, `CategoryName` and `TotalFinishers` on results | My Results shows "47th of 312 finishers" |

`IImageStorage` / `BlobStorageService` keep the Azure code in one place, and the new endpoints have unit tests that use
a fake storage class, so the tests never touch Azure.
