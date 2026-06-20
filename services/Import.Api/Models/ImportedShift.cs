namespace Timegrip.Import.Api.Models;

internal sealed class ImportedShift
{
    public int RowNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ImportedShiftRequirement> ShiftRequirements { get; set; } = [];
}
