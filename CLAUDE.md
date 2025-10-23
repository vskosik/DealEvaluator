# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8 web application for real estate deal evaluation, built using Clean Architecture principles with a focus on analyzing properties through integration with the Zillow API.

## Architecture

The solution follows Clean Architecture with clear separation of concerns across multiple projects:

- **DealEvaluator.Domain**: Core business entities, enums, and domain logic
- **DealEvaluator.Application**: Application services, DTOs, and business logic orchestration
- **DealEvaluator.Infrastructure**: Data access, external APIs, repositories, Entity Framework configurations
- **DealEvaluator.Web**: ASP.NET Core MVC presentation layer with Razor views
- **DealEvaluator.Tests**: XUnit test project with in-memory database testing

### Key Architectural Patterns

- **Repository Pattern**: Generic `IRepository<T>` and `DbRepository<T>` for data access
- **Service Registration**: Extension methods in each layer (`AddInfrastructureServices`)
- **Entity Framework**: Code-first with explicit configurations in separate files
- **ASP.NET Identity**: For user authentication and authorization
- **Clean Architecture**: Domain entities inherit from `RealEstateEntity` base class

## Development Commands

Since .NET SDK is not available in the current environment, typical commands would be:

```bash
# Build the solution
dotnet build

# Run the web application
dotnet run --project DealEvaluator.Web

# Run all tests
dotnet test

# Run specific test project
dotnet test DealEvaluator.Tests

# Apply database migrations
dotnet ef database update --project DealEvaluator.Infrastructure --startup-project DealEvaluator.Web

# Create new migration
dotnet ef migrations add MigrationName --project DealEvaluator.Infrastructure --startup-project DealEvaluator.Web
```

## Key Components

### Database Context
- `DealEvaluatorContext` with Entity Framework configurations in separate files
- Uses SQL Server with connection string from configuration
- Entity configurations located in `DealEvaluator.Infrastructure/Configurations/`

### External API Integration
- `ZillowApiService` handles property search via RapidAPI
- Requires `RAPID_API_KEY` environment variable
- DTOs for Zillow API requests/responses in `DealEvaluator.Application/DTOs/Zillow/`

### Authentication
- ASP.NET Core Identity with `User` entity extending `IdentityUser`
- Identity API endpoints mapped automatically
- Authorization configured but seeding code is currently commented out

### Testing
- XUnit with in-memory database for integration tests
- `PropertyCrudTests` demonstrates CRUD operations testing pattern
- Uses `UseInMemoryDatabase` for isolated test scenarios

## Configuration

- Connection strings in `appsettings.json`
- API keys via environment variables (security best practice)
- User secrets configured for development environment
- Swagger/OpenAPI enabled for development

## Important Files

- `Program.cs`: Application startup and service configuration
- `DealEvaluatorContext.cs`: Main database context
- `InfrastructureServiceRegistration.cs`: Infrastructure dependency injection setup
- Property entities use enums for `PropertyTypes` and `PropertyConditions`