# Insurance Backend API

A .NET 8 REST API for managing building-insurance clients, properties, policies, broker administration, pricing configuration, and policy reports. The solution uses a layered architecture with ASP.NET Core, Entity Framework Core, and SQLite.

## Features

- Client and building management, including paged client search
- Country, county, and city lookup data
- Insurance policy creation, premium calculation, activation, cancellation, and search
- Broker, currency, fee, and risk-factor administration
- Policy reports grouped by country, county, city, or broker, with query filters
- Admin authentication with JWT bearer tokens
- Centralized API exception handling
- Swagger UI in Development and Testing environments
- NUnit unit and integration tests

## Solution structure

| Project | Responsibility |
| --- | --- |
| `Insurance.Backend` | ASP.NET Core API, controllers, authentication, middleware, Swagger, and startup configuration |
| `Insurance.Application` | Application services, DTOs, validation, mapping, and application rules |
| `Insurance.Domain` | Domain entities, enums, and core domain types |
| `Insurance.Infrastructure` | EF Core `AppDbContext`, SQLite configuration, repositories, and development data seeding |
| `Insurance.Tests` | NUnit unit and API integration tests |

## Requirements

- .NET 8 SDK
- An IDE or command-line shell that can run .NET applications

SQLite is included through the project dependencies, so a separate database server is not required for the default local setup.

## Run locally

From the repository root (the directory containing `Insurance.Backend.sln`):

```bash
dotnet restore Insurance.Backend.sln
dotnet build Insurance.Backend.sln
dotnet run --project Insurance.Backend/Insurance.Api.csproj
```

The API creates the SQLite database and seeds initial reference data on startup. By default, the database file is `insurance.db` in the process working directory. The seeder adds Romania, sample counties and cities, RON and EUR currencies, fee and risk-factor configuration, a default broker, and a development admin account.

When running with the `Development` environment, Swagger UI is available at:

```text
https://localhost:<port>/swagger
http://localhost:<port>/swagger
```

Use the HTTPS or HTTP port printed by `dotnet run` or shown in the API project's launch profile.

## Configuration

The API reads JWT settings from the `Jwt` section in `Insurance.Backend/appsettings.json`. Configure a strong signing key of at least 32 characters and set appropriate issuer, audience, and token lifetime values for your environment.

The SQLite connection can be overridden with the standard .NET configuration key:

```text
ConnectionStrings__DefaultConnection="Data Source=insurance.db"
```

For local development, use .NET User Secrets or environment variables for secrets. Do not commit production credentials or signing keys. The checked-in JWT key is a placeholder and must be replaced before deployment.

## Authentication

Request an admin JWT token:

```http
POST /api/auth/admin/login
Content-Type: application/json
```

```json
{
  "email": "admin@local",
  "password": "Admin123!"
}
```

A successful response contains an `accessToken`. In Swagger, click **Authorize** and enter the token as a bearer token, or send it in API requests as:

```http
Authorization: Bearer <accessToken>
```

The default admin account is created by the development seeder so the local API can be explored immediately. Change or remove this behavior and use secure account provisioning before exposing the application beyond a local development environment.

## API areas

| Area | Base route | Access |
| --- | --- | --- |
| Admin login | `/api/auth/admin` | Public login endpoint |
| Clients | `/api/brokers/clients` | Broker-facing API |
| Buildings | `/api/brokers/clients/{clientId}/buildings` and `/api/brokers/buildings` | Broker-facing API |
| Geography | `/api/brokers` | Country, county, and city lookups |
| Policies | `/api/brokers/policies` | Broker-facing API |
| Broker administration | `/api/admin/brokers` | Admin JWT required |
| Currency administration | `/api/admin/currencies` | Admin JWT required |
| Fee administration | `/api/admin/fees` | Admin JWT required |
| Risk factors | `/api/admin/risk-factors` | Admin JWT required |
| Reports | `/api/admin/reports` | Admin JWT required |

For exact request and response schemas, use Swagger while the API is running.

## Run tests

Run the full test suite from the solution root:

```bash
dotnet test Insurance.Backend.sln
```

The tests include unit coverage for application services and integration coverage for API endpoints and policy workflows. Integration tests use the test host and a test database configuration; they do not require a separate SQLite server.

## CI

The repository includes `.gitlab-ci.yml` with build and test stages. It restores and builds the solution with the .NET 8 SDK, runs tests with coverage collection, and publishes test and coverage artifacts.

## Notes

- Startup uses `EnsureCreatedAsync` rather than EF Core migrations. If the schema changes, handle existing local database files accordingly or introduce migrations before relying on long-lived databases.
- The seeded credentials and sample data are for local development only.
