using HrSystem.Domain.Entities;

namespace HrSystem.Infrastructure.Jwt
{
    public interface IJwtTokenGenerator
    {
         public string GenerateToken(User user);
    }
}
