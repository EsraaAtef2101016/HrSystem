using System;
using System.Threading.Tasks;
using HrSystem.Domain.Entities;
using HrSystem.Infrastructure.IRepository;
using HrSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Infrastructure.IRepository.Repository;

public class PublicHolidayRepository : Repository<PublicHoliday>, IPublicHolidayRepository
{
    public PublicHolidayRepository(ApplicationDBContext context) : base(context)
    {
    }

    public async Task<PublicHoliday?> GetByIdAsync(Guid id)
    {
        return await _context.PublicHolidays.FirstOrDefaultAsync(ph => ph.Id == id);
    }

    public async Task<IEnumerable<PublicHoliday>> GetFutureHolidaysAsync(DateOnly startDate)
    {
        return await _context.PublicHolidays
            .Where(h => h.Date >= startDate)
            .OrderBy(h => h.Date)
            .ToListAsync();
    }
    public async Task<IEnumerable<PublicHoliday>> GetAllHolidaysAsync()
    {
        return await _context.PublicHolidays
            .OrderBy(h => h.Date)
            .ToListAsync();
    }
}
