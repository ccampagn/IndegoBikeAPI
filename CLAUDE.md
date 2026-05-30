# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
dotnet build                  # Build
dotnet run --project IndegoBikeAPI  # Run (HTTP on :5232, HTTPS on :7270)
dotnet publish                # Publish
```

No test project exists yet. The file `IndegoBikeAPI/IndegoBikeAPI.http` contains sample HTTP requests usable in VS Code REST Client or JetBrains HTTP Client.

## Architecture

ASP.NET Core 9.0 Web API targeting the Indego bike-share system (Philadelphia). Currently scaffolded from the default template — only the placeholder `WeatherForecast` endpoint exists; Indego-specific controllers and services have not been added yet.

**Key files:**
- `IndegoBikeAPI/Program.cs` — app startup, middleware pipeline, service registration
- `IndegoBikeAPI/Controllers/` — API controllers (attribute-routed)
- `IndegoBikeAPI/appsettings.json` — logging and host configuration

**Conventions:**
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`)
- Implicit usings are enabled
- OpenAPI/Swagger is exposed only in the Development environment at `/openapi/v1.json`

When adding Indego-specific features, controllers go in `Controllers/`, models/DTOs in a `Models/` directory, and external HTTP client services in a `Services/` directory.
