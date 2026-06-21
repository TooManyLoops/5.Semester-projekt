using Microsoft.Extensions.Caching.Memory;
using Timegrip.Gateway.Api.Downstream;
using Timegrip.Gateway.Api.Downstream.Models;

namespace Timegrip.Gateway.Api.Aggregation.Services;

public sealed class RoleCatalogService(
    EmployeesApiClient employeesApi,
    IMemoryCache memoryCache
)
{
    private const string RolesCacheKey = "role-catalog";
    private static readonly TimeSpan RolesCacheDuration = TimeSpan.FromMinutes(30);

    // Caches only the role catalog, not which roles individual employees have.
    public Task<List<RoleDto>> GetRoles(CancellationToken cancellationToken) =>
        memoryCache.GetOrCreateAsync(
            RolesCacheKey,
            async entry =>
            {
                // Absolute expiration is a safety net if a role write bypasses gateway invalidation.
                entry.AbsoluteExpirationRelativeToNow = RolesCacheDuration;
                return await employeesApi.GetRoles(cancellationToken);
            }
        )!;

    public void InvalidateRoles()
    {
        // Role writes through the gateway remove stale catalog data immediately.
        memoryCache.Remove(RolesCacheKey);
    }
}
