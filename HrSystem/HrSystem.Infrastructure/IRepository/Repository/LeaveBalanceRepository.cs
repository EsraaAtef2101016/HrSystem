using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.IRepository.Repository;

public class LeaveBalanceRepository : Repository<LeaveBalance>, ILeaveBalanceRepository
{
    private readonly ApplicationDBContext _dbContext;

    public LeaveBalanceRepository(ApplicationDBContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<LeaveBalance?> GetByEmployeeAndTypeAndYearAsync(Guid employeeId, LeaveType leaveType, int year)
    {
        return await _dbContext.LeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveType == leaveType && b.Year == year);
    }

    public async Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _dbContext.LeaveBalances
            .Where(b => b.EmployeeId == employeeId)
            .ToListAsync();
    }

    public new async Task<IEnumerable<LeaveBalance>> GetAllAsync()
    {
        return await _dbContext.LeaveBalances
            .Include(b => b.Employee)
            .ToListAsync();
    }

    public new async Task<LeaveBalance?> GetByIdAsync(Guid id)
    {
        return await _dbContext.LeaveBalances
            .Include(b => b.Employee)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
