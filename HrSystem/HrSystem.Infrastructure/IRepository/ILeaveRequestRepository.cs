using HrSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.IRepository
{
    public interface ILeaveRequestRepository : IRepository<LeaveRequest>
    {
        Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId);
        new Task<IEnumerable<LeaveRequest>> GetAllAsync();
        new Task<LeaveRequest?> GetByIdAsync(Guid id);
        
        Task<User?> GetUserOfLeaveRequestAsync(Guid leaveRequestId);
        
    }
    
}