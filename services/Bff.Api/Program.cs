using Timegrip.Bff.Api.Downstream;
using Timegrip.Bff.Api.Endpoints;
using Timegrip.Bff.Api.Gateway.Services;
using Timegrip.Bff.Api.Observability;
using Timegrip.Bff.Api.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CorrelationContext>();
builder.Services.AddTransient<DownstreamLoggingHandler>();

builder.Services
    .AddHttpClient<EmployeesApiClient>(client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("EMPLOYEES_API_URL", "http://localhost:5001"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

builder.Services
    .AddHttpClient<ShiftsApiClient>(client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("SHIFTS_API_URL", "http://localhost:5002"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

builder.Services
    .AddHttpClient("EmployeesProxy", client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("EMPLOYEES_API_URL", "http://localhost:5001"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

builder.Services
    .AddHttpClient("ShiftsProxy", client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("SHIFTS_API_URL", "http://localhost:5002"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

builder.Services.AddScoped<EmployeeAggregationService>();
builder.Services.AddScoped<ShiftAggregationService>();
builder.Services.AddScoped<RoleCatalogService>();
builder.Services.AddScoped<ProxyService>();

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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/health",
    () => Results.Ok(new { Status = "Healthy", Service = "Aggregated API Gateway" })
);

app.MapAggregateEndpoints();
app.MapProxyEndpoints();

app.Run();

static string GetServiceBaseUrl(string environmentVariableName, string fallback)
{
    return Environment.GetEnvironmentVariable(environmentVariableName)?.TrimEnd('/') ?? fallback;
}

static string[] GetFrontendOrigins()
{
    return (Environment.GetEnvironmentVariable("FRONTEND_ORIGIN")
            ?? "http://localhost:62892;http://127.0.0.1:62892")
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public partial class Program;
