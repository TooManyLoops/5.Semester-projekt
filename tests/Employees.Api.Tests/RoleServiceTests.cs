using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Services;

namespace Employees.Api.Tests;

public sealed class RoleServiceTests
{
    [Fact]
    public async Task CreateRole_WhenRequestIsValid_ShouldPersistRole()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new RoleService(context);
        var request = new RoleRequest
        {
            Name = "Bartender",
            Description = "Can cover bar shifts",
        };

        // Act
        var role = await service.CreateRole(request);

        // Assert
        role.RoleId.Should().NotBeEmpty();
        role.Name.Should().Be("Bartender");
        context.Roles.Should().ContainSingle(storedRole => storedRole.RoleId == role.RoleId);
    }

    private static EmployeesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EmployeesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployeesDbContext(options);
    }
}
