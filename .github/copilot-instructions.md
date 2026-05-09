# MoneySaver.Identity agent context

## What this project does
- This repository hosts the **MoneySaver identity service**, an ASP.NET Core Web API on **.NET 9**.
- It manages **user authentication and account lifecycle** for the wider MoneySaver system using **ASP.NET Core Identity** and **Entity Framework Core with SQL Server**.
- The API currently exposes identity flows for:
  - `POST /Identity/Register`
  - `POST /Identity/Login`
  - `POST /Identity/validateToken`
  - `PUT /Identity/ChangePassword`
- JWTs are generated here and used for authenticated calls to other MoneySaver services.

## Core behavior
- Users are stored in `IdentityDbContext`.
- The custom `User` entity extends `IdentityUser` with a `UserState` enum:
  - `Inactive`
  - `Active`
- Login is blocked when a user is inactive.
- Registration creates the ASP.NET Identity user and then calls the Data API endpoint:
  - `POST {UrlRoutesConfiguration.DataApiUrl}/api/appConfiguration/setuserconfig`
- There is also an internal activation flow in `IdentityService.Activate(...)` that marks a user as active and creates the user configuration in the Data API.

## Architecture and important files
- `MoneySaver.Identity\Program.cs`
  - Configures the web app, DI, Swagger, Serilog, OpenTelemetry, and Prometheus scraping.
- `MoneySaver.Identity\Controllers\IdentityController.cs`
  - Thin HTTP layer for register, login, token validation, and password change.
- `MoneySaver.Identity\Services\Identity\IdentityService.cs`
  - Main business logic for registration, login, password change, and activation.
- `MoneySaver.Identity\Services\Identity\TokenGeneratorService.cs`
  - Generates and validates JWT tokens using `ApplicationSettings:Secret`.
- `MoneySaver.Identity\Data\IdentityDbContext.cs`
  - EF Core identity database context.
- `MoneySaver.Identity\Data\IdentityDataSeeder.cs`
  - Seeds the admin role and a default admin user on first startup.
- `MoneySaver.Identity\Infrastructure\ServiceCollectionExtensions.cs`
  - Configures ASP.NET Core Identity password rules and EF-backed user storage.

## Configuration that must usually exist
- `ConnectionStrings:DefaultConnection`
  - SQL Server connection for the identity database.
- `ApplicationSettings:Secret`
  - Symmetric key used for JWT signing and validation.
- `UrlRoutesConfiguration:DataApiUrl`
  - Base URL of the downstream Data API used to create per-user configuration.
- `Serilog`
  - Configured to write logs to MariaDB.

## Observability and platform details
- Uses **Serilog** for logging.
- Uses **OpenTelemetry** for traces and metrics.
- Exposes a **Prometheus** scraping endpoint.
- Depends on shared infrastructure from the `MoneySaver.System` package, including `AddWebService<TContext>()`, `UseWebService(...)`, result models, and current-user abstractions.

## Working guidance for AI agents
- Treat this as an **identity/authentication microservice**, not a general user-profile service.
- Keep `IdentityController` thin; prefer business changes in `Services\Identity`.
- When changing authentication behavior, check both:
  - token generation/validation
  - downstream Data API configuration creation
- Preserve the current registration/login contract unless the task explicitly changes the public API.
- Be careful with startup and seeding changes; the app currently creates an initial admin role and user when no roles exist.
- Reuse existing `MoneySaver.System.Services.Result` patterns instead of introducing new response wrappers.

## Useful commands
- Build:
  - `dotnet build .\MoneySaver.Identity\MoneySaver.Identity.csproj`
- Run:
  - `dotnet run --project .\MoneySaver.Identity\MoneySaver.Identity.csproj`

## Notes for future changes
- Nullable reference types are currently not enabled in the project file.
- The current build succeeds, but dependency restore reports security warnings for some packages; if a task touches package versions, review those warnings before updating dependencies.
