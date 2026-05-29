using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Endpoints;
using Timegrip.Shifts.Api.Services;
using Polly;
using Polly.Retry;
using Scalar.AspNetCore;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);


// Define a retry policy with exponential backoff
builder.Services.AddSingleton<AsyncRetryPolicy>(Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, retryCount) =>
        {
            Console.WriteLine($"Retrying request. Retry count: {retryCount}");
        }));

builder.Services.AddScoped<ShiftService>();
builder.Services.AddHttpClient<VerificationService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddDbContext<ShiftsDbContext>(options =>
    options.UseSqlServer(GetConnectionString(), sql =>
        sql.UseCompatibilityLevel(160)
            .EnableRetryOnFailure()
            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Shift")));

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(GetFrontendOrigins())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

ApplyMigrations<ShiftsDbContext>(app);

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapShiftEndpoints();

app.Run();

static string GetConnectionString()
{
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_SHIFT");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            $"CONNECTION_STRING_SHIFT not found. Current directory: {Directory.GetCurrentDirectory()}");
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
        "Shift",
        "Shifts",
        "20260511180347_InitialCreate"
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
