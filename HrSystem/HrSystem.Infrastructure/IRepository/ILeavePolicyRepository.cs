using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;

namespace HrSystem.Infrastructure.IRepository
{
    public interface ILeavePolicyRepository : IRepository<LeavePolicy>
    {
        Task<LeavePolicy?> GetByIdAsync(Guid id);
        Task<IEnumerable<LeavePolicy>> GetAllAsync();
        Task<LeavePolicy?> GetActiveByTypeAsync(LeaveType leaveType);
        Task<bool> HasBeenUsedAsync(Guid policyId);
    }
}