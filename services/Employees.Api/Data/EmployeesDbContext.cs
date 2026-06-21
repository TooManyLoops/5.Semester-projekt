using Microsoft.EntityFrameworkCore;
using Timegrip.Employees.Api.Models;

namespace Timegrip.Employees.Api.Data;

public class EmployeesDbContext(DbContextOptions<EmployeesDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Employment> Employments => Set<Employment>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Employee");
    }
}
