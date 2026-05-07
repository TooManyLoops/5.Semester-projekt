using Microsoft.EntityFrameworkCore;
using Vagtplanlægnings_modul.Models;

namespace Vagtplanlægnings_modul.Data;

public class TimegripDbContext : DbContext
{
    public TimegripDbContext(DbContextOptions<TimegripDbContext> options)
        : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Role> Roles { get; set; }   
    public DbSet<EmployeeRole> EmployeeRoles { get; set; }   
    public DbSet<Employment> Employments { get; set; }
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
    public DbSet<ShiftRequirement> ShiftRequirements { get; set; }
}
