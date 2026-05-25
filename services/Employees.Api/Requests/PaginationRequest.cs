namespace Timegrip.Employees.Api.Requests;

public class PaginationRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}