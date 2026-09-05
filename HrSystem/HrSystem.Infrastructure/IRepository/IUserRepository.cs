using HrSystem.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace HrSystem.Infrastructure.IRepository;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
}
