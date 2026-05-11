using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Endpoints;
using Timegrip.Shifts.Api.Services;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

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
        policy.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? "http://localhost:62892")
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

static void ApplyMigrations<TContext>(WebApplication app)
    where TContext : DbContext
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TContext>();
    context.Database.Migrate();
}
