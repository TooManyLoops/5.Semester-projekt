namespace Timegrip.Import.Api.Responses;

public sealed class ImportedShiftResponse
{
    public int RowNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ImportedShiftRequirementResponse> ShiftRequirements { get; set; } = [];
}
