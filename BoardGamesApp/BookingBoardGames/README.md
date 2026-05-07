BookingBoardGames - Local development and deployment

Overview
- This solution contains:
  - `BookingBoardGames.Data` (EF Core models & DbContext)
  - `BookingBoardGamesWeb` (ASP.NET Core API)
  - `BookingBoardGames` (WinUI client)

Prerequisites
- .NET 8 SDK
- SQL Server (or LocalDB)
- PowerShell (for provided scripts)

Quickstart
1) Configure the database connection
- Edit `BookingBoardGamesWeb/appsettings.json` and set `ConnectionStrings:DefaultConnection`.

2) Apply EF migrations
- From repository root run (PowerShell):
  - `./scripts/update-db.ps1`

3) Run the API
- From repository root run (PowerShell):
  - `./scripts/start-api.ps1`
- Open Swagger at `http://localhost:5000/swagger` to interact with endpoints.

4) Run the WinUI app
- Open the solution in Visual Studio, set `BookingBoardGames` as startup project and run (F5).
- The app uses the database via EF Core and the API for controllers.

Publishing the API
- `dotnet publish BookingBoardGamesWeb/BookingBoardGames.Api.csproj -c Release -o ./publish`
- Run the published output: `dotnet ./publish/BookingBoardGames.Api.dll`

Tests
- Run unit tests: `dotnet test`

Troubleshooting
- If migrations fail, verify connection string and that SQL Server is accessible.
- If Swagger not reachable, make sure no other process uses port 5000 or change the port in `Program.cs` and `App.BaseApiUrl`.

If you want CI/CD or Docker support added, tell me and I will add it.