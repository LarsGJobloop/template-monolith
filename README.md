# <MINIMAL TEMPLATE>

A minimal template for a multi service ASP .NET solution.

Content:

- Preconfigured Docker Compose manifest
  - Database service (PostgreSQL)
  - Ingress service (Traefik)
- A single .NET Service example
  - Controller-based API with automatic model validation
  - ATDD style test configured with TestEnvironment abstraction
  - DatabaseContext
  - Simple schema migration

## Setup

1. Create a new repository from this template
2. Verify host dependenciese are met
3. Start developing and making changes

   - Run tests:

     ```sh
     dotnet test
     ```

   - Start services:

     ```sh
     docker compose up
     ```

   - Clean up all resources:

     ```sh
     docker compose down --volumes --remove-orphans
     ```

## Notes

- When adding a new database (in [init-database.sql](/init-databases.sql)). You need to clear out the database volume (DELETING ALL DATA) using `docker compose down --volumes`. It's the simplest solution for development, though production would run a dedicated migration script.

## References

- [.NET 10.0]()
- [xUnit]()
- [Docker Desktop]()
- [Docker Compose]()
- [ASP .NET]()
- [PostgreSQL]()
