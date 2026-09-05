using System.Threading;
using System.Threading.Tasks;
using HrSystem.Infrastructure.IRepository;

namespace HrSystem.Infrastructure.IRepository;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IPublicHolidayRepository PublicHolidays { get; }
    IEmployeeParticipationRepository EmployeeParticipations { get; }
    ILeavePolicyRepository LeavePolicies { get; }
    ILeaveRequestRepository LeaveRequests { get; }
    ILeaveBalanceRepository LeaveBalances { get; }
    IGlobalPolicyRepository GlobalPolicies { get; }
    Task<int> SaveChangesAsync();
}
