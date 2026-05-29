using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Endpoints;
using Timegrip.Employees.Api.Services;
using Scalar.AspNetCore;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<EmployeeRoleService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddDbContext<EmployeesDbContext>(options =>
    options.UseSqlServer(
        GetConnectionString(),
        sql =>
            sql.UseCompatibilityLevel(160)
                .EnableRetryOnFailure()
                .MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Employee")
    )
);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(GetFrontendOrigins())
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var app = builder.Build();

ApplyMigrations<EmployeesDbContext>(app);

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapEmployeeEndpoints();
app.MapEmployeeRoleEndpoints();
app.MapRoleEndpoints();


app.Run();

static string GetConnectionString()
{
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_EMPLOYEE");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            $"CONNECTION_STRING_EMPLOYEE not found. Current directory: {Directory.GetCurrentDirectory()}"
        );
    }

    return connectionString;
}

static string[] GetFrontendOrigins()
{
    return (Environment.GetEnvironmentVariable("FRONTEND_ORIGIN")
            ?? "http://localhost:62892;http://127.0.0.1:62892")
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

static void ApplyMigrations<TContext>(WebApplication app)
    where TContext : DbContext
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TContext>();
    EnsureInitialMigrationHistoryForLegacySchema(
        context,
        "Employee",
        "Employees",
        "20260513210930_InitialEmployeeDb"
    );
    context.Database.Migrate();
}

static void EnsureInitialMigrationHistoryForLegacySchema(
    DbContext context,
    string schema,
    string existingTable,
    string migrationId
)
{
    if (!context.Database.CanConnect())
    {
        return;
    }

    var repairSql = $"""
        IF SCHEMA_ID(N'{schema}') IS NOT NULL
        AND OBJECT_ID(N'[{schema}].[{existingTable}]', N'U') IS NOT NULL
        BEGIN
            IF OBJECT_ID(N'[{schema}].[__EFMigrationsHistory]', N'U') IS NULL
            BEGIN
                CREATE TABLE [{schema}].[__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM [{schema}].[__EFMigrationsHistory]
                WHERE [MigrationId] = N'{migrationId}'
            )
            BEGIN
                INSERT INTO [{schema}].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES (N'{migrationId}', N'10.0.7');
            END;
        END;
        """;

    context.Database.ExecuteSqlRaw(repairSql);
}
