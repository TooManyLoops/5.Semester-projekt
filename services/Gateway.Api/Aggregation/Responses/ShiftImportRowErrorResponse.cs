namespace Timegrip.Gateway.Api.Aggregation.Responses;

public sealed class ShiftImportRowErrorResponse
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}
