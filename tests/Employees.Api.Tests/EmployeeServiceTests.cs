using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Enums;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Services;

namespace Employees.Api.Tests;

public sealed class EmployeeServiceTests
{
    [Fact]
    public async Task CreateEmployee_WhenEmailHasWhitespace_ShouldTrimEmailBeforeSaving()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new EmployeeService(context);
        var request = CreateEmployeeRequest(email: "  anna@example.com  ");

        // Act
        var employee = await service.CreateEmployee(request);

        // Assert
        employee.Email.Should().Be("anna@example.com");
        context.Employees.Single().Email.Should().Be("anna@example.com");
    }

    [Fact]
    public async Task CreateEmployee_WhenEmailAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new EmployeeService(context);
        await service.CreateEmployee(CreateEmployeeRequest(email: "anna@example.com"));

        // Act
        var act = () => service.CreateEmployee(CreateEmployeeRequest(email: "anna@example.com"));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("An employee with this email already exists.");
    }

    [Fact]
    public async Task GetAllEmployees_WhenPaginationIsProvided_ShouldReturnRequestedPageOrderedByFirstName()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new EmployeeService(context);
        await service.CreateEmployee(CreateEmployeeRequest(firstName: "Charlie", email: "charlie@example.com"));
        await service.CreateEmployee(CreateEmployeeRequest(firstName: "Anna", email: "anna@example.com"));
        await service.CreateEmployee(CreateEmployeeRequest(firstName: "Berta", email: "berta@example.com"));

        // Act
        var employees = await service.GetAllEmployees(new PaginationRequest
        {
            PageNumber = 2,
            PageSize = 1,
        });

        // Assert
        employees.Should().ContainSingle();
        employees[0].FirstName.Should().Be("Berta");
    }

    private static EmployeesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EmployeesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployeesDbContext(options);
    }

    private static EmployeeRequest CreateEmployeeRequest(
        string firstName = "Anna",
        string lastName = "Andersen",
        string email = "anna@example.com") =>
        new()
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = "12345678",
            EmployeeStatus = EmployeeStatus.Active,
        };
}
