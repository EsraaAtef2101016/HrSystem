using FluentResults;
namespace HrSystem.Application.Features.ProfileFeature.IService;

using HrSystem.Application.Features.ProfileFeature.DTO.RequestDto;
using HrSystem.Application.Features.ProfileFeature.DTO.ResponseDto;

public interface IProfileService
{
    Task<Result<UserProfileResponse>> GetUserProfileAsync(Guid id);
    Task<Result<UserProfileResponse>> GetMyProfileAsync();
    Task<Result<IEnumerable<UserProfileResponse>>> GetAllUsersAsync();
    Task<Result<UserProfileResponse>> UpdateUserManagerAsync(UpdateUserManagerRequest request);
    Task<Result<UserProfileResponse>> UpdateUserStatusAsync(UpdateUserStatusRequest request);
    Task<Result<UserProfileResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request);
    Task<Result<IEnumerable<UserProfileResponse>>> GetAllAdminsAsync();

}