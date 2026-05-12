using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Timegrip.Roles.Api.Data;
using Timegrip.Roles.Api.Endpoints;
using Timegrip.Roles.Api.Services;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<EmployeeRoleService>();
builder.Services.AddDbContext<RolesDbContext>(options =>
    options.UseSqlServer(GetConnectionString(), sql =>
        sql.UseCompatibilityLevel(160)
            .EnableRetryOnFailure()
            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Role")));

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

ApplyMigrations<RolesDbContext>(app);

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapRoleEndpoints();
app.MapEmployeeRoleEndpoints();

app.Run();

static string GetConnectionString()
{
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_ROLES");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            $"CONNECTION_STRING not found. Current directory: {Directory.GetCurrentDirectory()}");
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
