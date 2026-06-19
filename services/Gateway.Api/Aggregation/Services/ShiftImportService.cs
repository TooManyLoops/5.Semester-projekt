using Timegrip.Gateway.Api.Aggregation.Responses;
using Timegrip.Gateway.Api.Downstream;
using Timegrip.Gateway.Api.Downstream.Requests;

namespace Timegrip.Gateway.Api.Aggregation.Services;

public sealed class ShiftImportService(
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
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return new ShiftImportResponse
            {
                Errors =
                [
                    new ShiftImportRowErrorResponse
                    {
                        RowNumber = 0,
                        Message = "Upload skal være en .xlsx-fil.",
                    },
                ],
            };
        }

        await using var stream = file.OpenReadStream();
        var rows = XlsxWorksheetReader.ReadFirstWorksheet(stream);
        var roles = await roleCatalog.GetRoles(cancellationToken);
        var parseResult = ShiftImportParser.Parse(rows, roles);

        var response = new ShiftImportResponse
        {
            ParsedRows = parseResult.Shifts.Count,
            PastShiftRows = parseResult.Shifts.Count(shift => shift.StartTime < DateTime.Now),
            FirstShiftDate = parseResult.Shifts.Count == 0
                ? null
                : parseResult.Shifts.Min(shift => shift.StartTime),
            LastShiftDate = parseResult.Shifts.Count == 0
                ? null
                : parseResult.Shifts.Max(shift => shift.EndTime),
            MatchedRoles = parseResult.MatchedRoles,
            UnknownRoles = parseResult.UnknownRoles,
        };

        foreach (var error in parseResult.Errors)
        {
            if (error.Message.Contains("fortiden", StringComparison.OrdinalIgnoreCase))
            {
                response.Warnings.Add(error);
                response.SkippedRows++;
                continue;
            }

            response.Errors.Add(error);
        }

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

        foreach (var importedShift in parseResult.Shifts)
        {
            if (excludePast && importedShift.StartTime < DateTime.Now)
            {
                response.SkippedRows++;
                response.Warnings.Add(new ShiftImportRowErrorResponse
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
                response.Warnings.Add(new ShiftImportRowErrorResponse
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
                    ShiftRequirements = importedShift.ShiftRequirements,
                },
                cancellationToken
            );
            response.CreatedShifts++;
            existingKeys.Add(importedKey);
        }

        return response;
    }

    private static string GetShiftKey(ImportedShift shift)
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
