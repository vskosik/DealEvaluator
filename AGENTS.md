# Repository Guidelines

## Project Structure & Module Organization
- `DealEvaluator.Web/` is the ASP.NET Core MVC app with `Controllers/`, `Views/`, `Models/`, `Extensions/`, `Middleware/`, and static assets in `wwwroot/`.
- `DealEvaluator.Application/`, `DealEvaluator.Domain/`, and `DealEvaluator.Infrastructure/` hold the layered business logic, domain entities, and data access.
- `DealEvaluator.Tests/` contains xUnit tests (for example `PropertyCrudTests.cs`).
- Root files include `DealEvaluator.sln`, `Dockerfile`, and `docker-compose.yml`; app configuration lives in `DealEvaluator.Web/appsettings*.json`.

## Build, Test, and Development Commands
- `dotnet build DealEvaluator.sln` builds the full solution.
- `dotnet run --project DealEvaluator.Web/DealEvaluator.Web.csproj` runs the web app locally.
- `dotnet test DealEvaluator.Tests/DealEvaluator.Tests.csproj` runs the test suite.
- `docker compose up --build` starts SQL Server and the web app; set `RAPID_API_KEY` in your environment for the container.

## Coding Style & Naming Conventions
- C# nullable reference types are enabled; keep nullability annotations accurate.
- Use `PascalCase` for public types/methods and `camelCase` for locals/parameters; controller classes end with `Controller`.
- Indentation is 4 spaces in existing files; no repo-wide formatter config is present, so match surrounding style.

## Testing Guidelines
- Tests use xUnit with EF Core InMemory; name new tests `*Tests.cs` and keep test methods focused on one behavior.
- Add coverage for data access and business rules alongside CRUD paths.

## Commit & Pull Request Guidelines
- Recent commits use short, sentence-style summaries without prefixes (example: "Added cost breakdown to evaluations").
- PRs should describe the change, note any DB/config updates, and include screenshots for UI changes in `Views/` or `wwwroot/`.

## Security & Configuration Tips
- Do not commit secrets; use user-secrets or environment variables for `ConnectionStrings:DealEvaluatorContext` and `RapidApiKey`.
- When using Docker, confirm connection strings and RapidAPI settings align with `docker-compose.yml`.
