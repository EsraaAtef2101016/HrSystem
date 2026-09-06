using System.Security.Claims;
using FluentResults;
using FluentValidation;
using HrSystem.Application.Common.DTO.ErrorDto;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.ResponseDto;
using HrSystem.Application.Features.LeaveRequestFeature.IService;
using HrSystem.Application.Features.LeaveReviewFeature.DTO.ResponseDto;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using HrSystem.Application.Extensions;
namespace HrSystem.Infrastructure.Service;

public class LeaveRequestService(
    IUnitOfWork unitOfWork,
    IValidator<CreateLeaveRequestRequest> createValidator,
    IValidator<UpdateLeaveRequestRequest> updateValidator,
    IHttpContextAccessor httpContextAccessor) : ILeaveRequestService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CreateLeaveRequestRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateLeaveRequestRequest> _updateValidator = updateValidator;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private Guid GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    public async Task<Result<IEnumerable<LeaveRequestResponse>>> GetAllAsync()
    {
        var requests = await _unitOfWork.LeaveRequests.GetAllAsync();
        return Result.Ok(requests.Select(MapToResponse));
    }

    public async Task<Result<IEnumerable<LeaveRequestResponse>>> GetMyRequestsAsync()
    {
        var userId = GetCurrentUserId();
        var requests = await _unitOfWork.LeaveRequests.GetByEmployeeIdAsync(userId);
        return Result.Ok(requests.Select(MapToResponse));
    }

    public async Task<Result<LeaveRequestResponse>> GetByIdAsync(Guid id)
    {
        var request = await _unitOfWork.LeaveRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Result.Fail<LeaveRequestResponse>(new NotFoundError("Leave request not found."));
        }
        return Result.Ok(MapToResponse(request));
    }


    public async Task<Result<LeaveRequestResponse>> CreateAsync(CreateLeaveRequestRequest request)
    {
        var validationResult = await CreateValidation(request);
        if (validationResult.IsFailed)
            return Result.Fail<LeaveRequestResponse>(validationResult.Errors);

        var policyResult = await ValidateLeaveRequestAsync(request.LeaveType, request.StartDate, request.EndDate);
        var activePolicy = policyResult.Value;
        // CreateAsync
        int chargedDays = await CalculateChargedBusinessDaysAsync(request.StartDate, request.EndDate);
        var userId = GetCurrentUserId();
        var leaveBalance = await _unitOfWork.LeaveBalances
            .GetByEmployeeAndTypeAndYearAsync(userId, request.LeaveType, request.StartDate.Year);
        if (leaveBalance == null)
            return Result.Fail<LeaveRequestResponse>(new NotFoundError("not Found ReviewLeaveResponse."));

        leaveBalance.ReserveDays(chargedDays);
        //leaveBalance.ReservedDays += chargedDays;
        _unitOfWork.LeaveBalances.Update(leaveBalance);

        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = userId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = LeaveStatus.Pending,
            ChargedDays = chargedDays,
            LeavePolicyId = activePolicy.Id,
            PolicyVersionSnapshot = activePolicy.Version,
            PolicyAllowanceSnapshot = activePolicy.AnnualAllowance,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveRequests.AddAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(leaveRequest));
    }

    public async Task<Result<LeaveRequestResponse>> CreateValidation(CreateLeaveRequestRequest request)
    {
        var validationResult = await _createValidator.ValidateRequestAsync<CreateLeaveRequestRequest, LeaveRequestResponse>(request);
        if (validationResult.IsFailed)
            return Result.Fail<LeaveRequestResponse>(validationResult.Errors);

        var policyResult = await ValidateLeaveRequestAsync(request.LeaveType, request.StartDate, request.EndDate);
        if (policyResult.IsFailed) return Result.Fail<LeaveRequestResponse>(policyResult.Errors);

        var activePolicy = policyResult.Value;
        // CreateAsync
        int chargedDays = await CalculateChargedBusinessDaysAsync(request.StartDate, request.EndDate);
        var userId = GetCurrentUserId();

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return Result.Fail<LeaveRequestResponse>(new NotFoundError("User not found."));

        if (!user.IsActive)
            return Result.Fail<LeaveRequestResponse>(new ConflictError("Leave request not Active."));
        return Result.Ok();
    }

    public async Task<Result<LeaveRequestResponse>> UpdateAsync(Guid id, UpdateLeaveRequestRequest request)
    {

        var validationResult = await UpdateValidation(id, request);
        if (validationResult.IsFailed)
            return Result.Fail<LeaveRequestResponse>(validationResult.Errors);

        var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(id);
        var userId = GetCurrentUserId();

        var policyResult = await ValidateLeaveRequestAsync(request.LeaveType, request.StartDate, request.EndDate, excludeRequestId: id);

        var activePolicy = policyResult.Value;
        // UpdateAsync
        int newChargedDays = await CalculateChargedBusinessDaysAsync(request.StartDate, request.EndDate);
        var leaveBalance = await _unitOfWork.LeaveBalances
            .GetByEmployeeAndTypeAndYearAsync(userId, request.LeaveType, request.StartDate.Year);

        if (leaveRequest == null)
            return Result.Fail<LeaveRequestResponse>(new NotFoundError("not Found ReviewLeaveResponse."));

        if (leaveBalance != null)
        {
            // Replace the raw assignment line with:
           leaveBalance.UpdateReservedDays(leaveRequest.ChargedDays, newChargedDays);
          //  leaveBalance.ReservedDays = (leaveBalance.ReservedDays - leaveRequest.ChargedDays) + newChargedDays;
            _unitOfWork.LeaveBalances.Update(leaveBalance);
        }

        leaveRequest.LeaveType = request.LeaveType;
        leaveRequest.LeavePolicyId = activePolicy.Id;
        leaveRequest.PolicyVersionSnapshot = activePolicy.Version;
        leaveRequest.PolicyAllowanceSnapshot = activePolicy.AnnualAllowance;
        leaveRequest.StartDate = request.StartDate;
        leaveRequest.EndDate = request.EndDate;
        leaveRequest.ChargedDays = newChargedDays;

        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(leaveRequest));
    }



    public async Task<Result<LeaveRequestResponse>> UpdateValidation(Guid id, UpdateLeaveRequestRequest request)
    {
        var validationResult = await _updateValidator.ValidateRequestAsync<UpdateLeaveRequestRequest, LeaveRequestResponse>(request);
        if (validationResult.IsFailed)
            return Result.Fail<LeaveRequestResponse>(validationResult.Errors);

        var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(id);
        if (leaveRequest == null)
            return Result.Fail<LeaveRequestResponse>(new NotFoundError("Leave request not found."));

        var userId = GetCurrentUserId();
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return Result.Fail<LeaveRequestResponse>(new NotFoundError("User not found."));

        if (!user.IsActive)
            return Result.Fail<LeaveRequestResponse>(new ForbiddenError("Leave request not Active."));
        if (userId != leaveRequest.EmployeeId)
            return Result.Fail<LeaveRequestResponse>(new ForbiddenError("You do not have permission to update this leave request."));

        if (leaveRequest.Status != LeaveStatus.Pending)
            return Result.Fail<LeaveRequestResponse>(new ConflictError("Only pending leave requests can be updated."));

        var policyResult = await ValidateLeaveRequestAsync(request.LeaveType, request.StartDate, request.EndDate, excludeRequestId: id);
        if (policyResult.IsFailed)
            return Result.Fail<LeaveRequestResponse>(policyResult.Errors);
        return Result.Ok();
    }

    public async Task<Result<MessageResponse>> CancelAsync(Guid id)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(id);
        if (leaveRequest == null)
            return Result.Fail(new NotFoundError("Leave request not found."));

        var validationResult = await ValidateCancelRequestAsync(leaveRequest);
        if (validationResult.IsFailed)
            return Result.Fail(validationResult.Errors);

        var leaveBalance = await _unitOfWork.LeaveBalances
            .GetByEmployeeAndTypeAndYearAsync(leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.StartDate.Year);

        if (leaveBalance != null)
        {
            if (leaveRequest.Status == LeaveStatus.Approved)
                leaveBalance.ReleaseUsedDays (leaveRequest.ChargedDays);
            if(leaveRequest.Status == LeaveStatus.Pending)  
                 leaveBalance.ReleaseReservedDays ( leaveRequest.ChargedDays);
          //  leaveBalance.UsedDays = Math.Max(0, leaveBalance.UsedDays - leaveRequest.ChargedDays);
            _unitOfWork.LeaveBalances.Update(leaveBalance);
        }

        leaveRequest.Status = LeaveStatus.Cancelled;
        _unitOfWork.LeaveRequests.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(new MessageResponse
        {
            Status = "Success",
            Message = "Cancelled successfully."
        });
    }

    private async Task<Result> ValidateCancelRequestAsync(LeaveRequest leaveRequest)
    {
        var userId = GetCurrentUserId();
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (leaveRequest.Status == LeaveStatus.Pending)
        {
            if (userId != leaveRequest.EmployeeId)
            {
                return Result.Fail(new ForbiddenError("You do not have permission to cancel this pending leave request."));
            }
        }
        else if (leaveRequest.Status == LeaveStatus.Approved)
        {
            bool isAdmin = user?.Role == UserRole.Admin;
            bool isOwner = userId == leaveRequest.EmployeeId;

            if (!isAdmin && !isOwner)
            {
                return Result.Fail(new ForbiddenError("You do not have permission to cancel this approved leave request."));
            }

            if (leaveRequest.StartDate <= today)
            {
                return Result.Fail(new BadRequestError("A started or historical approved request cannot be cancelled."));
            }
        }
        else
        {
            return Result.Fail(new BadRequestError("Only pending or future approved leave requests can be cancelled."));
        }

        return Result.Ok();
    }

    private async Task<Result<LeavePolicy>> ValidateLeaveRequestAsync(
        LeaveType leaveType,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeRequestId = null)
    {
        if (startDate > endDate)
        {
            return Result.Fail<LeavePolicy>(new BadRequestError("Start date cannot be after end date."));
        }

        if (startDate.Year != endDate.Year)
        {
            return Result.Fail<LeavePolicy>(new BadRequestError("Requests may not cross a calendar-year boundary."));
        }

        var activePolicy = await _unitOfWork.LeavePolicies.GetActiveByTypeAsync(leaveType);
        if (activePolicy == null)
        {
            return Result.Fail<LeavePolicy>(new ConflictError("No active or enabled policy found for this leave type."));
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (leaveType == LeaveType.SickLeave)
        {
            var minAllowedDate = today.AddDays(-activePolicy.BackdateDays);
            if (startDate < minAllowedDate)
            {
                return Result.Fail<LeavePolicy>(new BadRequestError($"Sick leave cannot be backdated more than {activePolicy.BackdateDays} days."));
            }
        }
        else
        {
            if (startDate < today)
            {
                return Result.Fail<LeavePolicy>(new BadRequestError("Start date cannot be in the past."));
            }

            var minNoticeDate = today.AddDays(activePolicy.MinNoticeDays);
            if (startDate < minNoticeDate)
            {
                return Result.Fail<LeavePolicy>(new BadRequestError($"Leave request must be submitted at least {activePolicy.MinNoticeDays} days in advance according MinNoticeDays."));
            }
        }

        // ValidateLeaveRequestAsync
        int chargedDays = await CalculateChargedBusinessDaysAsync(startDate, endDate);
        if (chargedDays <= 0)
        {
            return Result.Fail<LeavePolicy>(new BadRequestError($"A request that contains zero chargeable business days is invalid ({chargedDays})."));
        }

        if (chargedDays > activePolicy.AnnualAllowance)
        {
            return Result.Fail<LeavePolicy>(new BadRequestError("Requested leave days exceed the annual allowance."));
        }

        if (chargedDays > activePolicy.MaxConsecutiveDays)
        {
            return Result.Fail<LeavePolicy>(new BadRequestError("Requested leave days exceed the maximum consecutive days allowed."));
        }

        var myRequestsResult = await GetMyRequestsAsync();
        if (myRequestsResult.IsSuccess)
        {
            var existingRequests = myRequestsResult.Value
                .Where(r => r.Status == "Pending" || r.Status == "Approved")
                .Where(r => excludeRequestId == null || r.Id != excludeRequestId.Value)
                .ToList();

            var hasOverlap = existingRequests
                .Any(r => r.StartDate <= endDate && r.EndDate >= startDate);

            if (hasOverlap)
            {
                return Result.Fail<LeavePolicy>(new BadRequestError("You have an overlapping leave request."));
            }

            var combinedStart = startDate;
            var combinedEnd = endDate;
            bool merged;

            do
            {
                merged = false;
                foreach (var req in existingRequests)
                {
                    if (req.EndDate.AddDays(1) >= combinedStart && req.StartDate <= combinedStart)
                    {
                        if (req.StartDate < combinedStart)
                        {
                            combinedStart = req.StartDate;
                            merged = true;
                        }
                    }
                    if (req.StartDate.AddDays(-1) <= combinedEnd && req.EndDate >= combinedEnd)
                    {
                        if (req.EndDate > combinedEnd)
                        {
                            combinedEnd = req.EndDate;
                            merged = true;
                        }
                    }
                }
            } while (merged);

            int totalConsecutiveDays = combinedEnd.DayNumber - combinedStart.DayNumber + 1;
            if (totalConsecutiveDays > activePolicy.MaxConsecutiveDays)
            {
                return Result.Fail<LeavePolicy>(new BadRequestError($"Total consecutive leave days ({totalConsecutiveDays}) exceed the maximum allowed limit of {activePolicy.MaxConsecutiveDays} days."));
            }
        }

        int currentYear = startDate.Year;
        var currentUserId = GetCurrentUserId();
        var leaveBalance = await _unitOfWork.LeaveBalances
            .GetByEmployeeAndTypeAndYearAsync(currentUserId, leaveType, currentYear);

        if (leaveBalance == null)
        {
            leaveBalance =  new LeaveBalance(
            employeeId: currentUserId,
            leaveType: leaveType,
            year: currentYear,
            initialAllowance: activePolicy.AnnualAllowance
        );
            await _unitOfWork.LeaveBalances.AddAsync(leaveBalance);
        }

        int availableDays = leaveBalance.InitialAllowance - (leaveBalance.UsedDays + leaveBalance.ReservedDays);

        if (excludeRequestId.HasValue)
        {
            var oldRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(excludeRequestId.Value);
            if (oldRequest != null && oldRequest.LeaveType == leaveType)
            {
                availableDays += oldRequest.ChargedDays;
            }
        }

        if (chargedDays > availableDays)
        {
            return Result.Fail<LeavePolicy>(new BadRequestError($"Requested leave days ({chargedDays}) exceed your available balance ({availableDays} days)."));
        }

        return Result.Ok(activePolicy);
    }

    private LeaveRequestResponse MapToResponse(LeaveRequest request) => new()
    {
        Id = request.Id,
        EmployeeId = request.EmployeeId,
        LeaveType = request.LeaveType.ToString(),
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        Status = request.Status.ToString(),
        ChargedDays = request.ChargedDays,
        RejectionReason = request.RejectionReason,
        PolicyVersionSnapshot = request.PolicyVersionSnapshot,
        PolicyAllowanceSnapshot = request.PolicyAllowanceSnapshot,
        CreatedAt = request.CreatedAt
    };

    public async Task<Result<List<LeaveBalanceSummaryResponse>>> GetMyBalancesAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Result.Fail<List<LeaveBalanceSummaryResponse>>(new UnauthorizedError("User is not authenticated."));
        }

        int currentYear = DateTime.Today.Year;
        var resultList = new List<LeaveBalanceSummaryResponse>();

        foreach (LeaveType leaveType in Enum.GetValues(typeof(LeaveType)))
        {
            var policy = await _unitOfWork.LeavePolicies.GetActiveByTypeAsync(leaveType);
            if (policy == null) continue;

            var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndTypeAndYearAsync(userId, leaveType, currentYear);

            int initialAllowance = balance?.InitialAllowance ?? policy.AnnualAllowance;
            int usedDays = balance?.UsedDays ?? 0;
            int reservedDays = balance?.ReservedDays ?? 0;
            int availableDays = initialAllowance - (usedDays + reservedDays);

            resultList.Add(new LeaveBalanceSummaryResponse
            {
                LeaveType = leaveType.ToString(),
                InitialAllowance = initialAllowance,
                UsedDays = usedDays,
                ReservedDays = reservedDays,
                AvailableDays = availableDays,
                Year = currentYear
            });
        }

        return Result.Ok(resultList);
    }

    public async Task<int> CalculateChargedBusinessDaysAsync(DateOnly startDate, DateOnly endDate)
    {

        int businessDays = 0;

        var publicHolidays = await _unitOfWork.PublicHolidays.GetFutureHolidaysAsync(startDate);
        var holidayDates = publicHolidays.Select(h => h.Date).ToHashSet();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            bool isWeekend = date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;
            bool isHoliday = holidayDates.Contains(date);

            if (!isWeekend && !isHoliday)
            {
                businessDays++;
            }
        }
        return businessDays;
    }
}