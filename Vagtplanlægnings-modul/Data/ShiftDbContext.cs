using Microsoft.EntityFrameworkCore;
using Vagtplanlægnings_modul.Models;

namespace Vagtplanlægnings_modul.Data;

public class ShiftDbContext : DbContext
{
    public ShiftDbContext(DbContextOptions<ShiftDbContext> options)
        : base(options) { }

    public DbSet<Shift> Shifts { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }

}
