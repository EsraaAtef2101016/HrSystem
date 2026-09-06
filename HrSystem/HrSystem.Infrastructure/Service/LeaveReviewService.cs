using System.Security.Cryptography;
using FluentResults;
using HrSystem.Application.Common.DTO.ErrorDto;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using  HrSystem.Application.Features.LeaveReviewFeature.IService;
using HrSystem.Application.Features.LeaveReviewFeature.DTO.ResponseDto;
namespace HrSystem.Infrastructure.Service;

public class LeaveReviewService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : ILeaveReviewService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    public async Task<Result<ReviewLeaveResponse>> AcceptAsync(Guid leaveRequestId)
    {
        var requestValidation = await AcceptValidation(leaveRequestId);
        if (requestValidation.IsFailed)
            return Result.Fail<ReviewLeaveResponse>(requestValidation.Errors);

        var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId);
         if(leaveRequest ==null)
            return Result.Fail<ReviewLeaveResponse>(new NotFoundError("not Found ReviewLeaveResponse."));
       
        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        var currentYear = leaveRequest.StartDate.Year;
        var allBalances = await _unitOfWork.LeaveBalances.GetAllAsync();
        var leaveBalance = allBalances
            .FirstOrDefault(b => b.EmployeeId == leaveRequest.EmployeeId &&
                               b.LeaveType == leaveRequest.LeaveType &&
                               b.Year == currentYear);

        if (leaveBalance != null)
        {
            leaveBalance.CommitReservedToUsed(leaveRequest.ChargedDays);
           // leaveBalance.ReservedDays -= leaveRequest.ChargedDays;
           // leaveBalance.UsedDays += leaveRequest.ChargedDays;

            _unitOfWork.LeaveBalances.Update(leaveBalance);
        }

        _unitOfWork.LeaveRequests.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(leaveRequest));
    }

    public async Task<Result<ReviewLeaveResponse>> AcceptValidation(Guid leaveRequestId)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId);
        if (leaveRequest == null)
            return Result.Fail<ReviewLeaveResponse>(new NotFoundError("Leave request does not exist."));

        if (leaveRequest.Employee == null)
            leaveRequest.Employee = await _unitOfWork.LeaveRequests.GetUserOfLeaveRequestAsync(leaveRequestId);

        if (leaveRequest.Status != LeaveStatus.Pending)
            return Result.Fail<ReviewLeaveResponse>(new ConflictError("Only pending requests can be approved or rejected."));

        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
            return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("User is not authenticated."));

        var user = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
        if (user == null)
            return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("User not found."));

        var currentUserRole = user.Role;
        if (currentUserRole == UserRole.Manager)
        {
            if (leaveRequest.Employee?.ManagerId != currentUserId)
                return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("Manager cannot act outside their own team requests."));

            if (leaveRequest.EmployeeId == currentUserId)
                return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("Managers cannot approve their own requests. Admin approval required."));
        }
        else if (currentUserRole == UserRole.Admin)
        {
            if (leaveRequest.EmployeeId == currentUserId)
            {
                return Result.Fail<ReviewLeaveResponse>(new BadRequestError("Admin cannot approve their own requests."));
            }

            // Admin can only review requests if the employee is a Manager OR has no Manager (ManagerId is null)
            bool isManager = leaveRequest.Employee?.Role == UserRole.Manager;
            bool hasNoManager = leaveRequest.Employee?.ManagerId == null;

            if (!isManager && !hasNoManager)
            {
                return Result.Fail<ReviewLeaveResponse>(new ForbiddenError("Admins can only review requests for managers or employees without a manager."));
            }
        }
        else
        {
            return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("Unauthorized to review leave requests."));
        }

        return Result.Ok<ReviewLeaveResponse>(default!);
    }

    public async Task<Result<ReviewLeaveResponse>> RejectAsync(Guid leaveRequestId, string rejectionReason)
    {
        var requestValidation = await RejectValidation(leaveRequestId);
        if (requestValidation.IsFailed)
            return Result.Fail<ReviewLeaveResponse>(requestValidation.Errors);

        var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId);
        if(leaveRequest ==null)
            return Result.Fail<ReviewLeaveResponse>(new NotFoundError("not Found ReviewLeaveResponse."));
       
        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.RejectionReason = rejectionReason;

        var currentYear = leaveRequest.StartDate.Year;
        var allBalances = await _unitOfWork.LeaveBalances.GetAllAsync();
        var leaveBalance = allBalances
            .FirstOrDefault(b => b.EmployeeId == leaveRequest.EmployeeId &&
                               b.LeaveType == leaveRequest.LeaveType &&
                               b.Year == currentYear);

        if (leaveBalance != null)
        {
            leaveBalance.ReleaseReservedDays(leaveRequest.ChargedDays);
          //  leaveBalance.ReservedDays -= leaveRequest.ChargedDays;
            _unitOfWork.LeaveBalances.Update(leaveBalance);
        }

        leaveRequest.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.LeaveRequests.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(leaveRequest));
    }

    public async Task<Result<ReviewLeaveResponse>> RejectValidation(Guid leaveRequestId)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            return Result.Fail<ReviewLeaveResponse>(new NotFoundError("Leave request does not exist."));
        }

        if (leaveRequest.Employee == null)
        {
            leaveRequest.Employee = await _unitOfWork.LeaveRequests.GetUserOfLeaveRequestAsync(leaveRequestId);
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            return Result.Fail<ReviewLeaveResponse>(new ConflictError("Only pending requests can be approved or rejected. Action already processed."));
        }

        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("User is not authenticated."));
        }

        var user = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
        if (user == null)
        {
            return Result.Fail<ReviewLeaveResponse>(new NotFoundError("User not found."));
        }

        var currentUserRole = user.Role;

        if (currentUserRole == UserRole.Manager)
        {
            if (leaveRequest.Employee?.ManagerId != currentUserId)
            {
                return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("Manager cannot act outside their own team requests."));
            }

            if (leaveRequest.EmployeeId == currentUserId)
            {
                return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("Managers cannot approve their own requests. Admin approval required."));
            }
        }
        else if (currentUserRole == UserRole.Admin)
        {

            if (leaveRequest.EmployeeId == currentUserId)
            {
                return Result.Fail<ReviewLeaveResponse>(new BadRequestError("Admin cannot approve their own requests."));
            }

            // Admin can only review requests if the employee is a Manager OR has no Manager (ManagerId is null)
            bool isManager = leaveRequest.Employee?.Role == UserRole.Manager;
            bool hasNoManager = leaveRequest.Employee?.ManagerId == null;

            if (!isManager && !hasNoManager)
            {
                return Result.Fail<ReviewLeaveResponse>(new ForbiddenError("Admins can only review requests for managers or employees without a manager."));
            }
        }
        else
        {
            return Result.Fail<ReviewLeaveResponse>(new UnauthorizedError("Unauthorized to review leave requests."));
        }

        return Result.Ok<ReviewLeaveResponse>(default!);
    }

    public async Task<Result<IEnumerable<ReviewLeaveResponse>>> GetManagerPendingRequestsAsync()
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Result.Fail<IEnumerable<ReviewLeaveResponse>>("User is not authenticated.");
        }

        var allRequests = await _unitOfWork.LeaveRequests.GetAllAsync();
        var pendingRequests = allRequests
            .Where(lr => lr.Status == LeaveStatus.Pending && lr.Employee != null && lr.Employee.ManagerId == currentUserId.Value);

        var response = pendingRequests.Select(lr => MapToResponse(lr));

        return Result.Ok(response);
    }

    public async Task<Result<IEnumerable<ReviewLeaveResponse>>> GetAllPendingRequestsForAdminAsync()
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Result.Fail<IEnumerable<ReviewLeaveResponse>>(new UnauthorizedError("User is not authenticated."));
        }

        var user = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
        if (user?.Role != UserRole.Admin)
        {
            return Result.Fail<IEnumerable<ReviewLeaveResponse>>(new ForbiddenError("Only Admins can access all pending requests."));
        }

        var allRequests = await _unitOfWork.LeaveRequests.GetAllAsync();
        var pendingRequests = allRequests.Where(lr => lr.Status == LeaveStatus.Pending);

        var response = pendingRequests.Select(lr => MapToResponse(lr));

        return Result.Ok(response);
    }

    private ReviewLeaveResponse MapToResponse(LeaveRequest leaveRequest) => new()
    {
        Id = leaveRequest.Id,
        Status = leaveRequest.Status.ToString(),
        EmployeeName = leaveRequest.Employee?.Name ?? string.Empty,
        UpdatedAt = leaveRequest.UpdatedAt ?? leaveRequest.CreatedAt,
        StartDate = leaveRequest.StartDate,
        EndDate = leaveRequest.EndDate
    };
}
