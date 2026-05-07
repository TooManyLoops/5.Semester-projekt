using Microsoft.EntityFrameworkCore;
using Timegrip.Shifts.Api.Models;

namespace Timegrip.Shifts.Api.Data;

public class ShiftsDbContext(DbContextOptions<ShiftsDbContext> options) : DbContext(options)
{
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<ShiftRequirement> ShiftRequirements => Set<ShiftRequirement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Shift");
    }
}
