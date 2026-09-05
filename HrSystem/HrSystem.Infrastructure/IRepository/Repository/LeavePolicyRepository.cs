using Microsoft.EntityFrameworkCore;
using HrSystem.Infrastructure.IRepository;
using HrSystem.Infrastructure.Persistence.Context;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;



using System;
using System.Threading.Tasks;
namespace HrSystem.Infrastructure.IRepository.Repository;

public class LeavePolicyRepository : Repository<LeavePolicy>, ILeavePolicyRepository
{
    private readonly ApplicationDBContext _context;

    public LeavePolicyRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<LeavePolicy?> GetByIdAsync(Guid id) =>
        await _context.Set<LeavePolicy>().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<LeavePolicy>> GetAllAsync() =>
        await _context.Set<LeavePolicy>().ToListAsync();

    public async Task<LeavePolicy?> GetActiveByTypeAsync(LeaveType leaveType) =>
        await _context.Set<LeavePolicy>().FirstOrDefaultAsync(p => p.LeaveType == leaveType && p.IsEnabled);

    public async Task<bool> HasBeenUsedAsync(Guid policyId)
    {
        // Implement check against LeaveRequests if the entity exists in your context
       return await _context.LeaveRequests.AnyAsync(r => r.LeavePolicyId == policyId); 
    }
}
