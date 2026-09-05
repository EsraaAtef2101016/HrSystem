using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.IRepository
{
    public interface ILeaveBalanceRepository : IRepository<LeaveBalance>
    {
        Task<LeaveBalance?> GetByEmployeeAndTypeAndYearAsync(Guid employeeId, LeaveType leaveType, int year);
        Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(Guid employeeId);
        new Task<IEnumerable<LeaveBalance>> GetAllAsync();
        new Task<LeaveBalance?> GetByIdAsync(Guid id);
    }
}