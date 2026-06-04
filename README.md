# TekkenStats — Setup Instructions

## Prerequisites
- .NET 10 SDK
- (Optional) SQLite viewer for inspecting the DB

## First-time setup

### 1. Copy files into your repo
Replace/merge the files from this package into your existing project structure.
The layout matches your existing Clean Architecture solution exactly.

### 2. Set your JWT secret
Open `TekkenStats.API/appsettings.json` and replace the placeholder:
```json
"Jwt": {
  "Key": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS"
}
```
Use at least 32 characters. In production, use environment variables or User Secrets instead.

### 3. Create the EF Core migration
From the solution root:
```bash
dotnet ef migrations add InitialCreate \
  --project TekkenStats.Infrastructure \
  --startup-project TekkenStats.API
```

### 4. Run the API
```bash
cd TekkenStats.API
dotnet run
```
The database is auto-migrated on startup. Swagger UI is at https://localhost:{port}/swagger.

---

## Architecture overview

```
TekkenStats.Domain          — Entities only, no dependencies
TekkenStats.Application     — Interfaces + Services + DTOs (depends on Domain)
TekkenStats.Infrastructure  — EF Core, Repositories, wavu.wiki ingestion (depends on Application)
TekkenStats.API             — Controllers, Program.cs (depends on Application + Infrastructure)
```

## API endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | /api/auth/register | — | Register a new user |
| POST | /api/auth/login | — | Login, get JWT |
| POST | /api/auth/link-polaris | JWT | Link your Polaris ID |
| GET | /api/players/search?name= | — | Search players by name |
| GET | /api/players/{id} | — | Player profile by internal ID |
| GET | /api/players/polaris/{polarisId} | — | Player profile by Polaris ID |
| GET | /api/players/{id}/matches | — | Paginated match history |
| GET | /api/players/leaderboard | — | Global leaderboard |
| GET | /api/bookmarks | JWT | Your bookmarked players |
| POST | /api/bookmarks | JWT | Bookmark a player |
| DELETE | /api/bookmarks/{playerId} | JWT | Remove a bookmark |

All paginated endpoints accept `?page=1&pageSize=20`.

## wavu.wiki background ingestion

On startup the `ReplayPollingService` (a .NET `BackgroundService`) will:
1. Walk backward through all available replay history (~30 days, ~3600 batches)
2. After catch-up, poll `/api/replays` every **2 minutes** for new matches

Each batch covers 700 seconds of replays (~5000 matches). The cursor position is
persisted in the `IngestionStates` table so restarts resume from where they left off.

## Database
SQLite by default (`tekkenstats.db` in the API project folder).
To switch to SQL Server or PostgreSQL, change the `AddDbContext` call in `Program.cs`
and swap the EF Core provider package in `TekkenStats.Infrastructure.csproj`.
