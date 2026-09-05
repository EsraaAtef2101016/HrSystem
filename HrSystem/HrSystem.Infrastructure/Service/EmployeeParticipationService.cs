using FluentResults;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Application.Features.EmployeeParticipation.DTO.RequestDto;
using HrSystem.Application.Features.EmployeeParticipation.IService;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using HrSystem.Application.Common.DTO.ErrorDto;

using HrSystem.Application.Features.EmployeeParticipation.DTO.ResponseDto;
namespace HrSystem.Infrastructure.Service;

public class EmployeeParticipationService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : IEmployeeParticipationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId)) return userId;
        return null;
    }

    private async Task<GlobalPolicy> GetOrCreateGlobalPolicyAsync()
    {
        var policies = await _unitOfWork.GlobalPolicies.GetAllAsync();
        var policy = policies.FirstOrDefault();
        if (policy == null)
        {
            policy = new GlobalPolicy(isSelfOptOutAllowed: true, cooldownDays: 0);
            await _unitOfWork.GlobalPolicies.AddAsync(policy);
            await _unitOfWork.SaveChangesAsync();
        }
        return policy;
    }

    public async Task<Result<ParticipationStatusResponse>> GetMyStatusAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Result.Fail<ParticipationStatusResponse>(new UnauthorizedError("User is not authenticated."));

        var participation = await _unitOfWork.EmployeeParticipations.GetByEmployeeIdAsync(userId.Value);
        if (participation == null)
        {
            participation = new EmployeeParticipation(userId.Value, defaultOptIn: true);
            await _unitOfWork.EmployeeParticipations.AddAsync(participation);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Ok(new ParticipationStatusResponse
        {
            IsOptedIn = participation.IsOptedIn,
            LastOptOutDate = participation.LastOptOutDate,
            CooldownEndDate = participation.CooldownEndDate
        });
    }
     public async Task<Result<ParticipationStatusResponse>> GetStatusByIdAsync(Guid userId)
    {
        var participation = await _unitOfWork.EmployeeParticipations.GetByEmployeeIdAsync(userId);
        if (participation == null)
        {
            participation = new EmployeeParticipation(userId, defaultOptIn: true);
            await _unitOfWork.EmployeeParticipations.AddAsync(participation);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Ok(new ParticipationStatusResponse
        {
            IsOptedIn = participation.IsOptedIn,
            LastOptOutDate = participation.LastOptOutDate,
            CooldownEndDate = participation.CooldownEndDate
        });
    }


    public async Task<Result<MessageResponse>> OptOutAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Result.Fail<MessageResponse>(new UnauthorizedError("User is not authenticated."));
          var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
    if (user == null)
        return Result.Fail<MessageResponse>(new NotFoundError("User not found."));
        var policy = await GetOrCreateGlobalPolicyAsync();

        if (!policy.IsSelfOptOutAllowed)
            return Result.Fail<MessageResponse>(new BadRequestError("Self opt-out is disabled by the administrator."));

        var participation = await _unitOfWork.EmployeeParticipations.GetByEmployeeIdAsync(userId.Value);
        if (participation == null)
        {
            participation = new EmployeeParticipation(userId.Value, defaultOptIn: true);
            await _unitOfWork.EmployeeParticipations.AddAsync(participation);
        }

        if (!participation.IsOptedIn)
        {
            return Result.Fail<MessageResponse>(new ConflictError("Employee is already opted out."));
        }

        if (participation.CooldownEndDate.HasValue && DateTime.UtcNow < participation.CooldownEndDate.Value)
        {
            return Result.Fail<MessageResponse>(new BadRequestError("Cannot opt out during the active cooldown period."));
        }

        var leaveRequests = await _unitOfWork.LeaveRequests.GetAllAsync();
        var userRequests = leaveRequests.Where(lr => lr.EmployeeId == userId.Value).ToList();

        var hasPending = userRequests.Any(lr => lr.Status == LeaveStatus.Pending);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasFutureApproved = userRequests.Any(lr => lr.Status == LeaveStatus.Approved && lr.StartDate > today);
        if (hasPending || hasFutureApproved)
        {
            return Result.Fail<MessageResponse>(new BadRequestError("Cannot opt out while there are pending requests or approved requests with a future start date."));
        }
        user.IsActive = false;
        _unitOfWork.Users.Update(user);
        participation.OptOut(policy.CooldownDays);
      
        _unitOfWork.EmployeeParticipations.Update(participation);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(new MessageResponse
        {
            Status = "Success",
            Message = "Opted out successfully."
        });
    }

    public async Task<Result<MessageResponse>> OptInAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Result.Fail<MessageResponse>(new UnauthorizedError("User is not authenticated."));
        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
    if (user == null)
        return Result.Fail<MessageResponse>(new NotFoundError("User not found."));
        var participation = await _unitOfWork.EmployeeParticipations.GetByEmployeeIdAsync(userId.Value);
        if (participation == null)
        {
            participation = new EmployeeParticipation(userId.Value, defaultOptIn: true);
            await _unitOfWork.EmployeeParticipations.AddAsync(participation);
        }

        if (participation.IsOptedIn)
        {
            return Result.Fail<MessageResponse>(new ConflictError("Employee is already opted in."));
        }

        if (participation.CooldownEndDate.HasValue && DateTime.UtcNow < participation.CooldownEndDate.Value)
        {
            return Result.Fail<MessageResponse>(new BadRequestError("Cannot opt in before the cooldown period ends."));
        }

        participation.OptIn();
        user.IsActive = true;
         _unitOfWork.Users.Update(user);
        _unitOfWork.EmployeeParticipations.Update(participation);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(new MessageResponse
        {
            Status = "Success",
            Message = "Opted in successfully."
        });
    }

    public async Task<Result<MessageResponse>> UpdateGlobalPolicyAsync(UpdateGlobalPolicyRequest dto)
    {
        var policy = await GetOrCreateGlobalPolicyAsync();
        policy.UpdateSettings(dto.IsSelfOptOutAllowed, dto.CooldownDays);

        _unitOfWork.GlobalPolicies.Update(policy);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(new MessageResponse
        {
            Status = "Success",
            Message = "Update Global Policy successfully."
        });
    }

    public async Task<Result<MessageResponse>> ForceChangeEmployeeParticipationAsync(Guid employeeId, ForceParticipationRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            return Result.Fail<MessageResponse>(new BadRequestError("A reason is required for forced participation changes."));

        var policy = await GetOrCreateGlobalPolicyAsync();

        var participation = await _unitOfWork.EmployeeParticipations.GetByEmployeeIdAsync(employeeId);
        var user = await _unitOfWork.Users.GetByIdAsync(employeeId);
    if (user == null)
        return Result.Fail<MessageResponse>(new NotFoundError("User not found."));
        if (participation == null)
        {
            participation = new EmployeeParticipation(employeeId, defaultOptIn: true);
            await _unitOfWork.EmployeeParticipations.AddAsync(participation);
        }

        if (participation.IsOptedIn == dto.ForceOptIn)
            return Result.Fail<MessageResponse>(new ConflictError("The employee is already in the requested state."));
        user.IsActive = dto.ForceOptIn;
        _unitOfWork.Users.Update(user);
       // participation.IsOptedIn = dto.ForceOptIn;
        participation.SetLastForceChange(dto.Reason.Trim(), DateTime.UtcNow);

        if (!dto.ForceOptIn)
        {
            participation.OptOut( policy.CooldownDays);
           
        }
        else
        {
            participation.OptIn();
        }

        _unitOfWork.EmployeeParticipations.Update(participation);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(new MessageResponse
        {
            Status = "Success",
            Message = "Change Status successfully."
        });
    }
}