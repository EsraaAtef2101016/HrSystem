using System.Threading.Tasks;
using FluentResults;

using HrSystem.Application.Features.UserFeature.DTO.RequestDto;
using HrSystem.Application.Features.UserFeature.DTO.ResponseDto;
namespace HrSystem.Application.Features.UserFeature.IService;

public interface IUserService
{
    Task<Result<RegisterUserResponse>> RegisterAsync(RegisterUserRequest request);
    public Task<Result<LoginUserResponse>> LoginAsync(LoginUserRequest request);

}