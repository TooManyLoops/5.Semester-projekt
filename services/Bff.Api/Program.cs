using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

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

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Bff.Api" }));

app.MapMethods(
    "/api/employees/{**path}",
    ["GET", "POST", "PUT", "PATCH", "DELETE"],
    (HttpContext context, IHttpClientFactory httpClientFactory, string? path) =>
        ProxyRequest(
            context,
            httpClientFactory,
            GetServiceBaseUrl("EMPLOYEES_API_URL", "http://localhost:5001"),
            "employees",
            path
        )
);

app.MapMethods(
    "/api/roles/{**path}",
    ["GET", "POST", "PUT", "PATCH", "DELETE"],
    (HttpContext context, IHttpClientFactory httpClientFactory, string? path) =>
        ProxyRequest(
            context,
            httpClientFactory,
            GetServiceBaseUrl("EMPLOYEES_API_URL", "http://localhost:5001"),
            "roles",
            path
        )
);

app.MapMethods(
    "/api/employee-roles/{**path}",
    ["GET", "POST", "PUT", "PATCH", "DELETE"],
    (HttpContext context, IHttpClientFactory httpClientFactory, string? path) =>
        ProxyRequest(
            context,
            httpClientFactory,
            GetServiceBaseUrl("EMPLOYEES_API_URL", "http://localhost:5001"),
            "employee-roles",
            path
        )
);

app.MapMethods(
    "/api/shifts/{**path}",
    ["GET", "POST", "PUT", "PATCH", "DELETE"],
    (HttpContext context, IHttpClientFactory httpClientFactory, string? path) =>
        ProxyRequest(
            context,
            httpClientFactory,
            GetServiceBaseUrl("SHIFTS_API_URL", "http://localhost:5002"),
            "shifts",
            path
        )
);

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

static async Task ProxyRequest(
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    string serviceBaseUrl,
    string routePrefix,
    string? path
)
{
    using var requestMessage = CreateProxyRequest(context, serviceBaseUrl, routePrefix, path);
    using var responseMessage = await httpClientFactory
        .CreateClient()
        .SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted
        );

    context.Response.StatusCode = (int)responseMessage.StatusCode;

    foreach (var header in responseMessage.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    foreach (var header in responseMessage.Content.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    context.Response.Headers.Remove("transfer-encoding");

    await responseMessage.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
}

static HttpRequestMessage CreateProxyRequest(
    HttpContext context,
    string serviceBaseUrl,
    string routePrefix,
    string? path
)
{
    var queryString = context.Request.QueryString.HasValue
        ? context.Request.QueryString.Value
        : string.Empty;
    var targetUri = new Uri($"{serviceBaseUrl}/{routePrefix}/{path}{queryString}");
    var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

    if (
        HttpMethods.IsPost(context.Request.Method)
        || HttpMethods.IsPut(context.Request.Method)
        || HttpMethods.IsPatch(context.Request.Method)
    )
    {
        requestMessage.Content = new StreamContent(context.Request.Body);

        if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
        {
            requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(
                context.Request.ContentType
            );
        }
    }

    foreach (var header in context.Request.Headers)
    {
        if (
            header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
            || header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
            || header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
        )
        {
            continue;
        }

        if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
        {
            requestMessage.Content?.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value.ToArray()
            );
        }
    }

    requestMessage.Headers.Host = null;
    return requestMessage;
}
