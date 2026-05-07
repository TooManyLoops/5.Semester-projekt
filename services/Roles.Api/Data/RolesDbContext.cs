using Microsoft.EntityFrameworkCore;
using Timegrip.Roles.Api.Models;

namespace Timegrip.Roles.Api.Data;

public class RolesDbContext(DbContextOptions<RolesDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Role");
    }
}
