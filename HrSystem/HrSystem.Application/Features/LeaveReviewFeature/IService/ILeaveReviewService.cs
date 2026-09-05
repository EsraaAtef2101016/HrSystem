using FluentResults;
using HrSystem.Application.Features.LeaveReviewFeature.DTO.ResponseDto;
namespace HrSystem.Application.Features.LeaveReviewFeature.IService;

public interface ILeaveReviewService
{
    Task<Result<ReviewLeaveResponse>> AcceptAsync(Guid leaveRequestId);
    Task<Result<ReviewLeaveResponse>> RejectAsync(Guid leaveRequestId, string rejectionReason);
    Task<Result<IEnumerable<ReviewLeaveResponse>>> GetManagerPendingRequestsAsync();
    Task<Result<IEnumerable<ReviewLeaveResponse>>> GetAllPendingRequestsForAdminAsync();
}