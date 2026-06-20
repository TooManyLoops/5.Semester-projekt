# Timegrip microservices

This folder contains the first split of the old scheduling module into bounded ASP.NET APIs.

## Services

- `Gateway.Api` is the frontend-facing aggregated API gateway. It keeps the existing `/api/*` proxy routes for CRUD compatibility and adds `/api/aggregate/*` endpoints that compose data from multiple internal services into frontend-specific read models.
- `Gateway.Api` caches the role catalog from `Employees.Api` in memory for 30 minutes and invalidates it after successful role writes through the gateway. Employee-role assignments are not cached.
- `Import.Api` owns third-party import format parsing. The current Excel upload simulates an external workforce-planning integration; `Gateway.Api` supplies known roles, calls `Import.Api` for parsing, and then orchestrates shift creation through `Shifts.Api`.
- `Employees.Api` owns employees, employments, and employee statuses.
- `Employees.Api` also owns roles and employee-role assignments.
- `Shifts.Api` owns shifts, shift assignments, and shift requirements.

The services currently share the same SQL Server instance from `docker-compose.yml`, but each service has its own EF Core DbContext and schema boundary. That keeps the first migration small while making it possible to move toward database-per-service later.

## Local ports

- Frontend: `http://localhost:62892`
- Gateway API: `http://localhost:5000`
- Import API: `http://localhost:5003`
- Employees API: `http://localhost:5001`
- Shifts API: `http://localhost:5002`

Run all services with:

```powershell
docker compose up -d --build
```

Build all service projects with:

```powershell
dotnet build .\Timegrip-Microservices.slnx
```
