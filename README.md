# <MINIMAL TEMPLATE>

A minimal template for an ASP .NET monolith application.

Content:

- Preconfigured Docker Compose manifest
  - Application service (monolith)
  - Database service (PostgreSQL)
  - Database administration UI (pgAdmin)
- Single .NET Application project
  - Entity Framework Core with PostgreSQL
  - Automatic database migrations on startup
  - Health check endpoint
  - ATDD style tests configured with TestEnvironment abstraction using Testcontainers

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/): Required for running the Docker Compose setup^1
- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0): Required for developing the application

^1 There are alternatives for running containers through a Compose manifest, but Docker Desktop is the most ubiquitous solution.

> [!TIP]
>
> While a container runtime is required on the host, other toolchains can be acquired through the Nix Flake Dev Shell. This is for the more adventurous, but saves time in the long run.

## Setup

1. Create a new repository from this template
2. Verify host dependencies are met
3. Start developing and making changes

   - Run tests:

     ```sh
     dotnet test
     ```

   - Start the application and dependencies:

     ```sh
     docker compose up
     ```

   - Clean up all resources:

     ```sh
     docker compose down --volumes --remove-orphans
     ```

> [!TIP]
>
> There's a couple of new CLIs in use here, so remember that most CLIs support `--help` to get a quick overview over what the various subcommands and flags are.

## Services

- **Application API**: http://localhost:8080
  - Health Check: http://localhost:8080/health

- **pgAdmin**: http://localhost:8081
  - PostgreSQL administration UI
  - Default credentials: admin@example.com / password
  - Connect to PostgreSQL using:
    - Host: postgres
    - Port: 5432
    - Username: postgres
    - Password: postgres

## Project Structure

```
src/
  Application/              # Main monolith application
    Infrastructure/
      Data/                 # Entity Framework DbContext
      Migrations/           # Database migrations
    Program.cs              # Application entry point
tests/
  Application.Spec/         # Integration tests with Testcontainers
```

## Notes

- Database migrations run automatically on application startup
- When modifying the database schema, create migrations using:
  ```sh
  dotnet ef migrations add ${MigrationName} --project src/Application
  ```
- If you need to add a new database in [init-databases.sql](/init-databases.sql), clear the database volume using `docker compose down --volumes` (this will delete all data).
- Tests use Testcontainers to spin up isolated PostgreSQL instances, ensuring test isolation and no interference between parallel test runs.

## References

- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [xUnit](https://xunit.net/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Docker Compose](https://docs.docker.com/compose/)
- [ASP .NET](https://dotnet.microsoft.com/apps/aspnet)
- [PostgreSQL](https://www.postgresql.org/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Testcontainers](https://testcontainers.com/)
