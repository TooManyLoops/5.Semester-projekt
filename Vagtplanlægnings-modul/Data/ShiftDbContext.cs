using Microsoft.EntityFrameworkCore;
using Vagtplanlægnings_modul.Models;

namespace Vagtplanlægnings_modul.Data;

public class ShiftDbContext : DbContext
{
    public ShiftDbContext(DbContextOptions<ShiftDbContext> options)
        : base(options) { }

    public DbSet<Shift> Shifts { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("shift");
        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.ToTable("EmployeeRoles", schema: "dbo", tb => tb.ExcludeFromMigrations()); // Ingen schema = dbo
            entity.HasKey(e => e.EmployeeRoleId);
            entity.Property(e => e.EmployeeRoleId).HasColumnName("EmployeeRole_Id");
        });
        modelBuilder.Entity<ShiftAssignment>(entity =>
        {
            entity.HasOne<EmployeeRole>()
                  .WithMany()
                  .HasForeignKey(sa => sa.EmployeeRoleId);
        });
    }
}
