using Microsoft.Extensions.Caching.Memory;
using Timegrip.Bff.Api.Downstream;
using Timegrip.Bff.Api.Downstream.Models;

namespace Timegrip.Bff.Api.Gateway.Services;

public sealed class RoleCatalogService(
    EmployeesApiClient employeesApi,
    IMemoryCache memoryCache
)
{
    private const string RolesCacheKey = "role-catalog";
    private static readonly TimeSpan RolesCacheDuration = TimeSpan.FromMinutes(30);

    public Task<List<RoleDto>> GetRoles(CancellationToken cancellationToken) =>
        memoryCache.GetOrCreateAsync(
            RolesCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = RolesCacheDuration;
                return await employeesApi.GetRoles(cancellationToken);
            }
        )!;

    public void InvalidateRoles()
    {
        memoryCache.Remove(RolesCacheKey);
    }
}
