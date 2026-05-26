using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Endpoints;
using Timegrip.Employees.Api.Services;

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
                .WithOrigins(
                    Environment.GetEnvironmentVariable("FRONTEND_ORIGIN")
                        ?? "http://localhost:62892"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var app = builder.Build();

//ApplyMigrations<EmployeesDbContext>(app);

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapEmployeeEndpoints();

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

static void ApplyMigrations<TContext>(WebApplication app)
    where TContext : DbContext
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TContext>();
    context.Database.Migrate();
}
