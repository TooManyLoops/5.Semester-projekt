using Timegrip.Bff.Api.Downstream;
using Timegrip.Bff.Api.Downstream.Models;
using Timegrip.Bff.Api.Gateway.Requests;
using Timegrip.Bff.Api.Gateway.Responses;

namespace Timegrip.Bff.Api.Gateway.Services;

public sealed class ShiftAggregationService(
    ShiftsApiClient shiftsApi,
    EmployeesApiClient employeesApi,
    RoleCatalogService roleCatalog
)
{
    public async Task<List<ShiftAggregateResponse>> GetShifts(CancellationToken cancellationToken)
    {
        var shiftsTask = shiftsApi.GetShifts(cancellationToken);
        var rolesTask = roleCatalog.GetRoles(cancellationToken);

        await Task.WhenAll(shiftsTask, rolesTask);

        var rolesById = (await rolesTask).ToDictionary(role => role.RoleId);

        return (await shiftsTask).Select(shift => ToAggregate(shift, rolesById)).ToList();
    }

    public async Task<ShiftAssignmentResult> AssignShift(
        Guid shiftId,
        AssignShiftGatewayRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!await employeesApi.EmployeeRoleExists(request.EmployeeRoleId, cancellationToken))
        {
            return ShiftAssignmentResult.EmployeeRoleNotFound;
        }

        await shiftsApi.AssignShift(shiftId, request, cancellationToken);
        return ShiftAssignmentResult.Assigned;
    }

    private static ShiftAggregateResponse ToAggregate(
        ShiftDto shift,
        IReadOnlyDictionary<Guid, RoleDto> rolesById
    )
    {
        return new ShiftAggregateResponse
        {
            ShiftId = shift.ShiftId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            ShiftRequirements = (shift.ShiftRequirements ?? [])
                .Select(requirement => ToRequirementAggregate(requirement, rolesById))
                .ToList(),
            ShiftAssignments = (shift.ShiftAssignments ?? [])
                .Select(ToAssignmentAggregate)
                .ToList(),
        };
    }

    private static ShiftRequirementAggregateResponse ToRequirementAggregate(
        ShiftRequirementDto requirement,
        IReadOnlyDictionary<Guid, RoleDto> rolesById
    )
    {
        rolesById.TryGetValue(requirement.RoleId, out var role);

        return new ShiftRequirementAggregateResponse
        {
            ShiftRequirementId = requirement.ShiftRequirementId,
            ShiftId = requirement.ShiftId,
            RoleId = requirement.RoleId,
            RoleName = role?.Name ?? "Unknown role",
            Amount = requirement.Amount,
        };
    }

    private static ShiftAssignmentAggregateResponse ToAssignmentAggregate(
        ShiftAssignmentDto assignment
    ) =>
        new()
        {
            ShiftAssignmentId = assignment.ShiftAssignmentId,
            ShiftId = assignment.ShiftId,
            EmployeeRoleId = assignment.EmployeeRoleId,
            Status = assignment.Status,
            AssignedAt = assignment.AssignedAt,
        };
}
