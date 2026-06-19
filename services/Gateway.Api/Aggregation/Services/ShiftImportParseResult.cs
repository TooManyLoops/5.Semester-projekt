using Timegrip.Gateway.Api.Aggregation.Responses;

namespace Timegrip.Gateway.Api.Aggregation.Services;

internal sealed class ShiftImportParseResult
{
    public List<ImportedShift> Shifts { get; set; } = [];
    public List<string> MatchedRoles { get; set; } = [];
    public List<string> UnknownRoles { get; set; } = [];
    public List<ShiftImportRowErrorResponse> Errors { get; set; } = [];
}
