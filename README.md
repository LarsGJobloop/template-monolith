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

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/): Required for running the Docker Compose setup^1
- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0): Required for developing the Services.

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

   - Start services:

     ```sh
     docker compose up
     ```

   - Clean up all resources:

     ```sh
     docker compose down --volumes --remove-orphans
     ```

## Services

All services are accessible through Traefik reverse proxy at the following addresses:

- **Example Service API**: http://example-service.localhost

  - Feature Flags API: http://example-service.localhost/api/feature-flags
  - Health Check: http://example-service.localhost/health

- **Traefik Dashboard**: http://traefik.localhost

  - View routing configuration and service status

- **pgAdmin**: http://pgadmin.localhost
  - PostgreSQL administration UI
  - Default credentials: admin@example.com / password

> **Note**: Ensure your `/etc/hosts` includes entries for `*.localhost` domains, or use a DNS service that resolves `*.localhost` to `127.0.0.1`.

## Notes

- When adding a new database (in [init-database.sql](/init-databases.sql)). You need to clear out the database volume (DELETING ALL DATA) using `docker compose down --volumes`. It's the simplest solution for development, though production would run a dedicated migration script.

## References

- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [xUnit](https://xunit.net/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Docker Compose](https://docs.docker.com/compose/)
- [ASP .NET](https://dotnet.microsoft.com/apps/aspnet)
- [PostgreSQL](https://www.postgresql.org/)
