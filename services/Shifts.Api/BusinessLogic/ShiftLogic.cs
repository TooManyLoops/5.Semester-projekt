using Timegrip.Shifts.Api.Requests;
using Timegrip.Shifts.Api.Responses;
using Timegrip.Shifts.Api.Services;

using System;

public class ShiftLogic
{
	public ShiftLogic(ShiftService service)	{}

    public async Task<ShiftResponse> CreateShift(ShiftRequest request)
    {
        if (request.Requirements is not null && request.Requirements.Any())
        {
            if (request.Requirements.Any(r =>
                r.RoleId == Guid.Empty ||
                r.Amount == 0))
                throw new ArgumentException("Alle felter i Requirements skal være udfyldt");            
            return await service.CreateShiftWithRequirements(request);
        }
        return await service.CreateShift(request);
    }
}
