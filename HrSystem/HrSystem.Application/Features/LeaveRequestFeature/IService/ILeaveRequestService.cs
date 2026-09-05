using FluentResults;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.ResponseDto;

namespace HrSystem.Application.Features.LeaveRequestFeature.IService;

public interface ILeaveRequestService
{
    Task<Result<IEnumerable<LeaveRequestResponse>>> GetAllAsync();
    Task<Result<IEnumerable<LeaveRequestResponse>>> GetMyRequestsAsync();
    Task<Result<LeaveRequestResponse>> GetByIdAsync(Guid id);
    Task<Result<LeaveRequestResponse>> CreateAsync(CreateLeaveRequestRequest request);
    Task<Result<LeaveRequestResponse>> UpdateAsync(Guid id, UpdateLeaveRequestRequest request);
    Task<Result<MessageResponse>>  CancelAsync(Guid id);
    Task<Result<List<LeaveBalanceSummaryResponse>>> GetMyBalancesAsync();
}