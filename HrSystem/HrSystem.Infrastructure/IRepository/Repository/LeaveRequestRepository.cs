using HrSystem.Domain.Entities;
using HrSystem.Infrastructure.IRepository;
using HrSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.IRepository.Repository;

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    private readonly ApplicationDBContext _dbContext;

    public LeaveRequestRepository(ApplicationDBContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _dbContext.LeaveRequests
            .Where(r => r.EmployeeId == employeeId)
            .Include(r => r.LeavePolicy)
            .Include(r => r.Employee)
            .ToListAsync();
    }

    public new async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        return await _dbContext.LeaveRequests
            .Include(r => r.LeavePolicy)
            .Include(r => r.Employee)
            .ToListAsync();
    }

    public new async Task<LeaveRequest?> GetByIdAsync(Guid id)
    {
        return await _dbContext.LeaveRequests
            .Include(r => r.LeavePolicy)
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<User?> GetUserOfLeaveRequestAsync(Guid leaveRequestId)
    {
        var leaveRequest = await _dbContext.LeaveRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == leaveRequestId);

        return leaveRequest?.Employee;
    }
}
