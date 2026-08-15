# SmartTaskOptimizer — Production-Grade Backend

A .NET 8 backend for a Jira-style project management platform. The backend is organized into API, Application, Domain, Infrastructure, Shared, and test projects.

## Architecture

- **API** — controllers, middleware, health/readiness endpoints, SignalR hub wiring, security headers, rate limiting.
- **Application** — MediatR commands/queries, validation, authorization-aware use cases, priority engine, authentication services.
- **Domain** — entities, enums, domain contracts, repository interfaces.
- **Infrastructure** — EF Core SQL Server persistence, repositories, SignalR notifier, Hangfire jobs, schema configuration.
- **Shared** — request/response DTOs and stable enums.
- **Tests** — unit tests and an API smoke test.

## Production capabilities

- JWT authentication and backend resource authorization.
- Project membership and manager/owner access control.
- Password hashing with ASP.NET Core Identity's `PasswordHasher<T>`.
- Centralized RFC 7807-style problem responses with validation details.
- SQL Server retry-on-failure and optimistic concurrency via `rowversion`.
- Indexed, filtered, sorted and paginated task queries.
- Soft deletion for projects/tasks/comments.
- Project members, tasks, statuses, priorities, tags, dependencies, comments, history, activity, notifications and reports.
- SignalR project/user notifications with connection authentication and project-room authorization.
- Hangfire background priority recalculation with configurable enablement.
- Global and endpoint-specific rate limiting.
- HTTPS redirection, HSTS in production, CORS allow-listing and security headers.
- Liveness (`/health`) and database readiness (`/ready`) endpoints.
- Environment-based secrets/configuration.

## Configuration

Required in production:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key` — at least 256 bits of entropy.
- `Jwt__Issuer`
- `Jwt__Audience`
- `Cors__Origins__0`, `Cors__Origins__1`, etc. for trusted frontend origins.

Optional:

- `Jwt__ExpirationMinutes` (default 60)
- `Hangfire__Enabled` (default true)
- `EPPlus__License` or `EPPlusLicense` for Excel export.

Never commit production secrets. Local overrides such as `appsettings.Local.json` are ignored by git.

## Database

The `database/schema.sql` script creates the current SQL Server schema. `database/legacy-migration.sql` contains compatibility steps for older `tbl*` naming where applicable.

For production deployments, use a controlled migration process and take a database backup before destructive schema changes.

## Real-time model

Clients authenticate the SignalR connection and explicitly join authorized project groups through `JoinProject(projectId)`. Important events include task creation/update/deletion, status changes, comments, member changes, project changes and user notifications.

For multiple API instances, use a shared SignalR backplane/pub-sub implementation rather than relying on process-local state.

## Verification

The source tree was statically reviewed and corrected for namespace inconsistencies, incomplete test dependencies, configuration/test startup issues, validation error reporting, atomic project creation, comment update/delete events, member-role validation, dependency authorization, and endpoint rate limiting.

The execution environment used for this delivery does **not** contain the .NET SDK, so `dotnet restore`, `dotnet build`, and `dotnet test` could not be executed here. Those commands must be run in a .NET 8 SDK environment before deployment.
