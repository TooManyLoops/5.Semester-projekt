using Timegrip.Import.Api.Models;
using Timegrip.Import.Api.Requests;
using Timegrip.Import.Api.Responses;

namespace Timegrip.Import.Api.Services;

public sealed class ShiftImportService
{
    public async Task<ShiftImportResponse> ParseShifts(
        IFormFile file,
        IReadOnlyCollection<RoleReferenceRequest> roles,
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
            ParsedShifts = parseResult.Shifts.Select(shift => new ImportedShiftResponse
            {
                RowNumber = shift.RowNumber,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                ShiftRequirements = shift.ShiftRequirements.Select(requirement => new ImportedShiftRequirementResponse
                {
                    RoleId = requirement.RoleId,
                    Amount = requirement.Amount,
                }).ToList(),
            }).ToList(),
        };

        foreach (var error in parseResult.Errors)
        {
            response.Errors.Add(error);
        }

        await Task.CompletedTask.WaitAsync(cancellationToken);
        return response;
    }
}
