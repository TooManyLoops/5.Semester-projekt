namespace Timegrip.Gateway.Api.Downstream.Responses;

public sealed class ShiftImportParseResponse
{
    public int ParsedRows { get; set; }
    public int SkippedRows { get; set; }
    public int PastShiftRows { get; set; }
    public DateTime? FirstShiftDate { get; set; }
    public DateTime? LastShiftDate { get; set; }
    public List<string> MatchedRoles { get; set; } = [];
    public List<string> UnknownRoles { get; set; } = [];
    public List<ShiftImportRowErrorResponse> Warnings { get; set; } = [];
    public List<ShiftImportRowErrorResponse> Errors { get; set; } = [];
    public List<ImportedShiftResponse> ParsedShifts { get; set; } = [];
    public bool IsValid => UnknownRoles.Count == 0 && Errors.Count == 0;
}
