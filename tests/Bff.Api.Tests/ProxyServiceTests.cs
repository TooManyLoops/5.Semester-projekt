using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Timegrip.Bff.Api.Downstream;
using Timegrip.Bff.Api.Gateway.Services;
using Timegrip.Bff.Api.Proxy;

namespace Bff.Api.Tests;

public sealed class ProxyServiceTests
{
    [Fact]
    public async Task ProxyRequest_WhenRoleWriteSucceeds_ShouldInvalidateCachedRoles()
    {
        var roleId = Guid.NewGuid();
        var employeesHandler = new StubHttpMessageHandler()
            .MapJson("/roles/", new[]
            {
                new { RoleId = roleId, Name = "Old role", Description = "Cached" },
            });
        var employeesApi = new EmployeesApiClient(new HttpClient(employeesHandler)
        {
            BaseAddress = new Uri("http://employees.test"),
        });
        var roleCatalog = new RoleCatalogService(
            employeesApi,
            new MemoryCache(new MemoryCacheOptions())
        );
        await roleCatalog.GetRoles(CancellationToken.None);

        var proxyHandler = new StubHttpMessageHandler()
            .MapStatus("/roles/", HttpStatusCode.Created);
        var proxyClient = new HttpClient(proxyHandler)
        {
            BaseAddress = new Uri("http://employees.test"),
        };
        var proxyService = new ProxyService(new StubHttpClientFactory(proxyClient), roleCatalog);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Response.Body = new MemoryStream();

        await proxyService.ProxyRequest(context, "EmployeesProxy", "roles", string.Empty);
        await roleCatalog.GetRoles(CancellationToken.None);

        employeesHandler.Requests
            .Count(request => request.RequestUri?.AbsolutePath == "/roles/")
            .Should()
            .Be(2);
    }
}
