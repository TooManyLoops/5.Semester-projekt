using Timegrip.Gateway.Api.Downstream.Requests;

namespace Timegrip.Gateway.Api.Aggregation.Services;

internal sealed class ImportedShift
{
    public int RowNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ShiftRequirementDownstreamRequest> ShiftRequirements { get; set; } = [];
}
