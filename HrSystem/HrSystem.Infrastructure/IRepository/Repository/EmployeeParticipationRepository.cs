using HrSystem.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.IRepository.Repository;

public class EmployeeParticipationRepository : Repository<EmployeeParticipation>, IEmployeeParticipationRepository
{
    private readonly ApplicationDBContext _context;

    public EmployeeParticipationRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<EmployeeParticipation?> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _context.EmployeeParticipations
            .FirstOrDefaultAsync(ep => ep.EmployeeId == employeeId);
    }
}
