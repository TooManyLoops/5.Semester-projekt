using Microsoft.EntityFrameworkCore;
using Vagtplanlægnings_modul.Data;
using Vagtplanlægnings_modul.Enums;
using Vagtplanlægnings_modul.Models;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Responses;

namespace Vagtplanlægnings_modul.Services
{
    public class ShiftService(ShiftDbContext context)
    {
        public async Task<string> CreateShift(ShiftRequest request)
        {
            var shift = new Shift()
            {
                ShiftId = request.ShiftId ?? Guid.NewGuid(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };
            context.Shifts.Add(shift);
            await context.SaveChangesAsync();
            return shift.ShiftId.ToString();
        }
    }
}
