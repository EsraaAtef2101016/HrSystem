using HrSystem.Infrastructure.IRepository;
using HrSystem.Infrastructure.Persistence.Context;
using System.Threading;
using System.Threading.Tasks;
using HrSystem.Domain.Entities;

namespace HrSystem.Infrastructure.IRepository.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _context;
        private readonly IUserRepository _users;
        private readonly IPublicHolidayRepository _publicHolidays;
        private readonly ILeavePolicyRepository _leavePolicies;
        private readonly ILeaveRequestRepository _leaveRequests;
        private readonly ILeaveBalanceRepository _leaveBalances;
        private readonly IEmployeeParticipationRepository _employeeParticipation;
        private readonly IGlobalPolicyRepository _globalPolicies;
        public IUserRepository Users => _users;
        public IPublicHolidayRepository PublicHolidays => _publicHolidays;
        public ILeavePolicyRepository LeavePolicies => _leavePolicies;
        public ILeaveRequestRepository LeaveRequests => _leaveRequests;
        public ILeaveBalanceRepository LeaveBalances => _leaveBalances;
        public IEmployeeParticipationRepository EmployeeParticipations =>  _employeeParticipation;
        
        public IGlobalPolicyRepository GlobalPolicies => _globalPolicies;
        public UnitOfWork(ApplicationDBContext context)
        {
            _context = context;
            _users = new UserRepository(_context);
            _publicHolidays = new PublicHolidayRepository(_context);
            _leavePolicies = new LeavePolicyRepository(_context);
            _leaveRequests = new LeaveRequestRepository(_context);
            _leaveBalances = new LeaveBalanceRepository(_context);
            _employeeParticipation = new EmployeeParticipationRepository(_context);
            _globalPolicies = new GlobalPolicyRepository(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}