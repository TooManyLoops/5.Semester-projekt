namespace Timegrip.Employees.Api.Requests;

pulbic class PaginationRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}