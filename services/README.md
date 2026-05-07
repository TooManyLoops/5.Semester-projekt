# Timegrip microservices

This folder contains the first split of the old scheduling module into three bounded ASP.NET APIs.

## Services

- `Bff.Api` is the frontend-facing backend-for-frontend that routes `/api/*` requests to the internal services.
- `Employees.Api` owns employees, employments, and employee statuses.
- `Roles.Api` owns roles and employee-role assignments.
- `Shifts.Api` owns shifts, shift assignments, and shift requirements.

The services currently share the same SQL Server instance from `docker-compose.yml`, but each service has its own EF Core DbContext and schema boundary. That keeps the first migration small while making it possible to move toward database-per-service later.

## Local ports

- BFF API: `http://localhost:5000`
- Employees API: `http://localhost:5001`
- Roles API: `http://localhost:5002`
- Shifts API: `http://localhost:5003`

Run all services with:

```powershell
docker compose up --build
```

Build all service projects with:

```powershell
dotnet build .\Timegrip-Microservices.slnx
```
