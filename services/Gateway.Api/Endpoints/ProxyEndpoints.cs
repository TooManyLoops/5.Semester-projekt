using Timegrip.Gateway.Api.Proxy;

namespace Timegrip.Gateway.Api.Endpoints;

public static class ProxyEndpoints
{
    public static WebApplication MapProxyEndpoints(this WebApplication app)
    {
        // These routes keep existing CRUD calls working while still forcing traffic through the gateway.
        app.MapMethods(
            "/api/employees/{**path}",
            ["GET", "POST", "PUT", "PATCH", "DELETE"],
            (HttpContext context, ProxyService proxyService, string? path) =>
                proxyService.ProxyRequest(context, "EmployeesProxy", "employees", path)
        );

        app.MapMethods(
            "/api/roles/{**path}",
            ["GET", "POST", "PUT", "PATCH", "DELETE"],
            (HttpContext context, ProxyService proxyService, string? path) =>
                proxyService.ProxyRequest(context, "EmployeesProxy", "roles", path)
        );

        app.MapMethods(
            "/api/employee-roles/{**path}",
            ["GET", "POST", "PUT", "PATCH", "DELETE"],
            (HttpContext context, ProxyService proxyService, string? path) =>
                proxyService.ProxyRequest(context, "EmployeesProxy", "employee-roles", path)
        );

        app.MapMethods(
            "/api/shifts/{**path}",
            ["GET", "POST", "PUT", "PATCH", "DELETE"],
            (HttpContext context, ProxyService proxyService, string? path) =>
                proxyService.ProxyRequest(context, "ShiftsProxy", "shifts", path)
        );

        return app;
    }
}
