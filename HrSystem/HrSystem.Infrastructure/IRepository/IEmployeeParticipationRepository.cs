using HrSystem.Domain.Entities;

namespace HrSystem.Infrastructure.IRepository;
public interface IEmployeeParticipationRepository : IRepository<EmployeeParticipation>
{
    Task<EmployeeParticipation?> GetByEmployeeIdAsync(Guid employeeId);
}
