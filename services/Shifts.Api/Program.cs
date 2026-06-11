using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Endpoints;
using Timegrip.Shifts.Api.Services;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Timeout;
using Polly.Fallback;
using Scalar.AspNetCore;
using System.Net;
using System.Threading.RateLimiting;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("ResilientClient")
    .AddResilienceHandler("full-pipeline", pipeline =>
    {
        // 1. FALLBACK – yderste lag, fanger alt der slipper igennem
        pipeline.AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: BrokenCircuitException }       => PredicateResult.True(),
                { Exception: TimeoutRejectedException }     => PredicateResult.True(),
                { Exception: RateLimiterRejectedException } => PredicateResult.True(),
                { Exception: HttpRequestException }         => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            FallbackAction = _ =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("Tjenesten er midlertidigt utilgængelig. Prøv igen senere.")
                };
                return ValueTask.FromResult(Outcome.FromResult(response));
            },
            OnFallback = args =>
            {
                Console.WriteLine($"⚠️ Fallback aktiveret: {args.Outcome.Exception?.Message}");
                return ValueTask.CompletedTask;
            }
        });

        // 2. RATE LIMITER – begræns antal udgående kald
        pipeline.AddRateLimiter(new SlidingWindowRateLimiter(
            new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10),
                SegmentsPerWindow = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 20
            }
        ));

        // 3. TIMEOUT – samlet timeout på tværs af alle forsøg
        pipeline.AddTimeout(new HttpTimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(30)
        });

        // 4. HEDGING – send parallelt request hvis svaret er langsomt
        pipeline.AddHedging(new HttpHedgingStrategyOptions
        {
            MaxHedgedAttempts = 2,
            Delay = TimeSpan.FromMilliseconds(500),
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException }                              => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.ServiceUnavailable }         => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            OnHedging = args =>
            {
                Console.WriteLine($"🔀 Hedging: sender parallelt request #{args.AttemptNumber}");
                return ValueTask.CompletedTask;
            }
        });

        // 5. CIRCUIT BREAKER – stop kald når servicen er nede
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            FailureRatio = 0.5,
            BreakDuration = TimeSpan.FromSeconds(15),
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException }                              => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.InternalServerError }        => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.ServiceUnavailable }         => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            OnOpened = args =>
            {
                Console.WriteLine($"⚡ Circuit ÅBNET – venter {args.BreakDuration.TotalSeconds}s");
                return ValueTask.CompletedTask;
            },
            OnClosed = _ =>
            {
                Console.WriteLine("✅ Circuit LUKKET igen");
                return ValueTask.CompletedTask;
            }
        });

        // 6. RETRY – prøv igen med eksponentiel backoff
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException }                              => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.TooManyRequests }            => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.InternalServerError }        => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.ServiceUnavailable }         => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            OnRetry = args =>
            {
                Console.WriteLine($"🔁 Retry #{args.AttemptNumber} om {args.RetryDelay.TotalSeconds:F1}s");
                return ValueTask.CompletedTask;
            }
        });

        // 7. TIMEOUT – per forsøg (inkl. hvert retry)
        pipeline.AddTimeout(new HttpTimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(10)
        });
    });

builder.Services.AddScoped<ShiftService>();
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