using FluentResults;
using HrSystem.Application.Features.LeavePolicyFeature.DTO.ResponseDto;
using HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;
namespace HrSystem.Application.Features.LeavePolicyFeature.IService;
//using HrSystem.Application.Features.LeavePolicyFeature.IService;
public interface ILeavePolicyService
{
    Task<Result<IEnumerable<LeavePolicyResponse>>> GetAllAsync();
    Task<Result<LeavePolicyResponse>> GetByIdAsync(Guid id);
    Task<Result<LeavePolicyResponse>> CreateAsync(CreateLeavePolicyRequest request);
    Task<Result<LeavePolicyResponse>> UpdateAsync(UpdateLeavePolicyRequest request);
    Task<Result<LeavePolicyResponse>> ToggleStatusAsync(ToggleStatusRequest request);
}