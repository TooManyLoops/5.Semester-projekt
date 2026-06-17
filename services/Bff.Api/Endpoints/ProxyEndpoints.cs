using Timegrip.Bff.Api.Proxy;

namespace Timegrip.Bff.Api.Endpoints;

public static class ProxyEndpoints
{
    public static WebApplication MapProxyEndpoints(this WebApplication app)
    {
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
