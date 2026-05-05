using Microsoft.EntityFrameworkCore;
using Vagtplanlægnings_modul.Models;

namespace Vagtplanlægnings_modul.Data;

public class TimegripDbContext : DbContext
{
    public TimegripDbContext(DbContextOptions<TimegripDbContext> options)
        : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Employment> Employments { get; set; }
}
