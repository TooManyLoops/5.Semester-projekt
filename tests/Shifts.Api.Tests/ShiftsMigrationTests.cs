using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Timegrip.Shifts.Api.Data;

namespace Shifts.Api.Tests;

public sealed class ShiftsMigrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }

    [Fact]
    public async Task MigrateAsync_WhenDatabaseIsEmpty_ShouldCreateShiftsSchema()
    {
        // Arrange
        await using var context = CreateContext();

        // Act
        await context.Database.MigrateAsync();

        // Assert
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        pendingMigrations.Should().BeEmpty();
        (await context.Shifts.AnyAsync()).Should().BeFalse();
    }

    private ShiftsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ShiftsDbContext>()
            .UseSqlServer(_database.GetConnectionString())
            .Options;

        return new ShiftsDbContext(options);
    }
}
