using Microsoft.EntityFrameworkCore;
using Vagtplanlægnings_modul.Data;
using Vagtplanlægnings_modul.Endpoints;
using Vagtplanlægnings_modul.Models;
using Vagtplanlægnings_modul.Services;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<EmployeeService>();

var ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(ConnectionString))
{
    throw new InvalidOperationException(
        $"CONNECTION_STRING not found. Current directory: {Directory.GetCurrentDirectory()}"
    );
}

builder.Services.AddDbContext<TimegripDbContext>(options =>
    options.UseSqlServer(ConnectionString, o => o.UseCompatibilityLevel(160))
);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:56284")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowAngularFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.MapEmployeeEndpoint();

app.Run();
