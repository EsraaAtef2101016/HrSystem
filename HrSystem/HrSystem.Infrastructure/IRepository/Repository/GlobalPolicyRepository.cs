using HrSystem.Domain.Entities;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace HrSystem.Infrastructure.IRepository.Repository;

public class GlobalPolicyRepository : Repository<GlobalPolicy>, IGlobalPolicyRepository
{
    private readonly ApplicationDBContext _context;

    public GlobalPolicyRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }
}
