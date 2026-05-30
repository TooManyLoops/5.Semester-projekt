using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Timegrip.Employees.Api.Data;

namespace Employees.Api.Tests;

public sealed class EmployeesMigrationTests : IAsyncLifetime
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
    public async Task MigrateAsync_WhenDatabaseIsEmpty_ShouldCreateEmployeesSchema()
    {
        // Arrange
        await using var context = CreateContext();

        // Act
        await context.Database.MigrateAsync();

        // Assert
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        pendingMigrations.Should().BeEmpty();
        (await context.Employees.AnyAsync()).Should().BeFalse();
    }

    private EmployeesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EmployeesDbContext>()
            .UseSqlServer(_database.GetConnectionString())
            .Options;

        return new EmployeesDbContext(options);
    }
}
