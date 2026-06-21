using Timegrip.Gateway.Api.Aggregation.Responses;
using Timegrip.Gateway.Api.Downstream;
using Timegrip.Gateway.Api.Downstream.Requests;
using Timegrip.Gateway.Api.Downstream.Responses;
using GatewayRowError = Timegrip.Gateway.Api.Aggregation.Responses.ShiftImportRowErrorResponse;
using ImportRowError = Timegrip.Gateway.Api.Downstream.Responses.ShiftImportRowErrorResponse;

namespace Timegrip.Gateway.Api.Aggregation.Services;

public sealed class ShiftImportService(
    ImportApiClient importApi,
    ShiftsApiClient shiftsApi,
    RoleCatalogService roleCatalog
)
{
    public async Task<ShiftImportResponse> ImportShifts(
        IFormFile file,
        bool includePast,
        bool excludePast,
        CancellationToken cancellationToken
    )
    {
        var roles = await roleCatalog.GetRoles(cancellationToken);
        var parseResult = await importApi.ParseShifts(file, roles, cancellationToken);

        var response = new ShiftImportResponse
        {
            ParsedRows = parseResult.ParsedRows,
            SkippedRows = parseResult.SkippedRows,
            PastShiftRows = parseResult.PastShiftRows,
            FirstShiftDate = parseResult.FirstShiftDate,
            LastShiftDate = parseResult.LastShiftDate,
            MatchedRoles = parseResult.MatchedRoles,
            UnknownRoles = parseResult.UnknownRoles,
            Warnings = parseResult.Warnings.Select(ToGatewayError).ToList(),
            Errors = parseResult.Errors.Select(ToGatewayError).ToList(),
        };

        if (!response.IsValid)
        {
            return response;
        }

        if (!includePast && !excludePast && response.PastShiftRows > 0)
        {
            response.RequiresPastConfirmation = true;
            return response;
        }

        var existingShifts = await shiftsApi.GetShifts(cancellationToken);
        var existingKeys = existingShifts.Select(GetShiftKey).ToHashSet();

        foreach (var importedShift in parseResult.ParsedShifts)
        {
            if (excludePast && importedShift.StartTime < DateTime.Now)
            {
                response.SkippedRows++;
                response.Warnings.Add(new GatewayRowError
                {
                    RowNumber = importedShift.RowNumber,
                    Message = "Historisk vagt blev sprunget over.",
                });
                continue;
            }

            var importedKey = GetShiftKey(importedShift);
            if (existingKeys.Contains(importedKey))
            {
                response.SkippedRows++;
                response.Warnings.Add(new GatewayRowError
                {
                    RowNumber = importedShift.RowNumber,
                    Message = "Vagten findes allerede og blev sprunget over.",
                });
                continue;
            }

            await shiftsApi.CreateShift(
                new CreateShiftDownstreamRequest
                {
                    StartTime = importedShift.StartTime,
                    EndTime = importedShift.EndTime,
                    AllowPast = true,
                    ShiftRequirements = importedShift.ShiftRequirements.Select(requirement =>
                        new ShiftRequirementDownstreamRequest
                        {
                            RoleId = requirement.RoleId,
                            Amount = requirement.Amount,
                        }
                    ).ToList(),
                },
                cancellationToken
            );

            response.CreatedShifts++;
            existingKeys.Add(importedKey);
        }

        return response;
    }

    private static GatewayRowError ToGatewayError(ImportRowError error) =>
        new()
        {
            RowNumber = error.RowNumber,
            Message = error.Message,
        };

    private static string GetShiftKey(ImportedShiftResponse shift)
    {
        var requirements = shift.ShiftRequirements
            .OrderBy(requirement => requirement.RoleId)
            .Select(requirement => $"{requirement.RoleId:N}:{requirement.Amount}");

        return $"{shift.StartTime:O}|{shift.EndTime:O}|{string.Join(",", requirements)}";
    }

    private static string GetShiftKey(Downstream.Models.ShiftDto shift)
    {
        var requirements = (shift.ShiftRequirements is { Count: > 0 }
                ? shift.ShiftRequirements
                : shift.RoleId is null
                    ? []
                    : [new Downstream.Models.ShiftRequirementDto { RoleId = shift.RoleId.Value, Amount = 1 }])
            .OrderBy(requirement => requirement.RoleId)
            .Select(requirement => $"{requirement.RoleId:N}:{requirement.Amount}");

        return $"{shift.StartTime:O}|{shift.EndTime:O}|{string.Join(",", requirements)}";
    }
}
