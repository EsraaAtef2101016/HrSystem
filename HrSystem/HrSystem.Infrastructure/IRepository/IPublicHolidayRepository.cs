using HrSystem.Domain.Entities;

namespace HrSystem.Infrastructure.IRepository
{
    public interface IPublicHolidayRepository : IRepository<PublicHoliday>
    {
        public Task<PublicHoliday?> GetByIdAsync(Guid id);
        public Task<IEnumerable<PublicHoliday>> GetFutureHolidaysAsync(DateOnly startDate);
        public Task<IEnumerable<PublicHoliday>> GetAllHolidaysAsync();
    }
}