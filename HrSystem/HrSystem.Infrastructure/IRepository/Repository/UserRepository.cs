using Microsoft.EntityFrameworkCore;
using HrSystem.Infrastructure.IRepository;
using HrSystem.Infrastructure.Persistence.Context;
using HrSystem.Domain.Entities;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.IRepository.Repository;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly ApplicationDBContext _context;

    public UserRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
}
