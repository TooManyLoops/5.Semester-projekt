# Timegrip microservices

This folder contains the first split of the old scheduling module into three bounded ASP.NET APIs.

## Services

- `Bff.Api` is the frontend-facing backend-for-frontend that routes `/api/*` requests to the internal services.
- `Employees.Api` owns employees, employments, and employee statuses.
- `Employees.Api` also owns roles and employee-role assignments.
- `Shifts.Api` owns shifts, shift assignments, and shift requirements.

The services currently share the same SQL Server instance from `docker-compose.yml`, but each service has its own EF Core DbContext and schema boundary. That keeps the first migration small while making it possible to move toward database-per-service later.

## Local ports

- Frontend: `http://localhost:62892`
- BFF API: `http://localhost:5000`
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
