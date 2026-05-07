using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Vagtplanlægnings_modul.Data;
using Vagtplanlægnings_modul.Endpoints;
using Vagtplanlægnings_modul.Models;
using Vagtplanlægnings_modul.Services;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<EmployeeService>();

builder.Services.AddScoped<RoleService>();

builder.Services.AddScoped<EmployeeRoleService>();

var ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(ConnectionString))
{
    throw new InvalidOperationException(
        $"CONNECTION_STRING not found. Current directory: {Directory.GetCurrentDirectory()}"
    );
}

builder.Services.AddDbContext<TimegripDbContext>(options =>
    options.UseSqlServer(ConnectionString, o => o.UseCompatibilityLevel(160).MigrationsHistoryTable(
            tableName: HistoryRepository.DefaultTableName,
            schema: "Employee"))
);

builder.Services.AddDbContext<ShiftDbContext>(options =>
    options.UseSqlServer(ConnectionString, o => o.UseCompatibilityLevel(160).MigrationsHistoryTable(
            tableName: HistoryRepository.DefaultTableName,
            schema: "Shift"))
);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:62892")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAngularFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.MapEmployeeEndpoint();

app.MapRoleEndpoint();

app.MapEmployeeRoleEndpoint();

app.MapShiftEndpoints();

app.Run();
