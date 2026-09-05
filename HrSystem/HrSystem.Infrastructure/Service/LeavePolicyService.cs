using FluentResults;
using FluentValidation;
using HrSystem.Application.Common.DTO.ErrorDto;
using HrSystem.Application.Common.DTO.ResponseDto;

using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using HrSystem.Application.Extensions;
using HrSystem.Application.Features.LeavePolicyFeature.IService;
using HrSystem.Application.Features.LeavePolicyFeature.DTO.ResponseDto;
using HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;
namespace HrSystem.Infrastructure.Service;

public class LeavePolicyService(
    IUnitOfWork unitOfWork,
    IValidator<CreateLeavePolicyRequest> createValidator,
    IValidator<UpdateLeavePolicyRequest> updateValidator) : ILeavePolicyService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CreateLeavePolicyRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateLeavePolicyRequest> _updateValidator = updateValidator;

    public async Task<Result<IEnumerable<LeavePolicyResponse>>> GetAllAsync()
    {
        var policies = await _unitOfWork.LeavePolicies.GetAllAsync();
        var response = policies.Select(MapToResponse);
        return Result.Ok(response);
    }

    public async Task<Result<LeavePolicyResponse>> GetByIdAsync(Guid id)
    {
        var policy = await _unitOfWork.LeavePolicies.GetByIdAsync(id);
        if (policy == null)
            return Result.Fail<LeavePolicyResponse>(new NotFoundError("Policy not found."));
        return Result.Ok(MapToResponse(policy));
    }

    public async Task<Result<LeavePolicyResponse>> CreateAsync(CreateLeavePolicyRequest request)
    {
        var validationResult = await _createValidator.ValidateRequestAsync<CreateLeavePolicyRequest, CreateLeavePolicyRequest>(request, StatusCodes.Status400BadRequest);
        if (validationResult.IsFailed)
        {
            return Result.Fail<LeavePolicyResponse>(validationResult.Errors);
        }
        if (!Enum.TryParse<LeaveType>(request.LeaveType, true, out var parsedLeaveType))
        {
        return Result.Fail<LeavePolicyResponse>(new BadRequestError("Invalid leave type format."));
        }

        var existingActive = await _unitOfWork.LeavePolicies.GetActiveByTypeAsync(parsedLeaveType);
        if (existingActive != null)
        {
            existingActive.IsEnabled = false;
            _unitOfWork.LeavePolicies.Update(existingActive);
        }

        var policy = new LeavePolicy
        {
            Id = Guid.NewGuid(),
            LeaveType = parsedLeaveType,
            AnnualAllowance = request.AnnualAllowance,
            MaxConsecutiveDays = request.MaxConsecutiveDays,
            MinNoticeDays = request.MinNoticeDays,
            BackdateDays = request.BackdateDays,
            IsEnabled = true,
            Version = existingActive != null ? existingActive.Version + 1 : 1
        };

        await _unitOfWork.LeavePolicies.AddAsync(policy);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(policy));
    }

    public async Task<Result<LeavePolicyResponse>> UpdateAsync(UpdateLeavePolicyRequest request)
    {
        var validationResult = await _updateValidator.ValidateRequestAsync<UpdateLeavePolicyRequest, LeavePolicyResponse>(request, StatusCodes.Status400BadRequest);
        if (validationResult.IsFailed)
        {
            return Result.Fail<LeavePolicyResponse>(validationResult.Errors);
        }

        var policy = await _unitOfWork.LeavePolicies.GetByIdAsync(request.id);
        if (policy == null)
            return Result.Fail<LeavePolicyResponse>(new NotFoundError("Policy not found."));
        if (!policy.IsEnabled)
            return Result.Fail<LeavePolicyResponse>(new BadRequestError("Cannot update a disabled policy."));


        bool isUsed = await _unitOfWork.LeavePolicies.HasBeenUsedAsync(request.id);

        if (isUsed)
        {
            policy.IsEnabled = false;
            _unitOfWork.LeavePolicies.Update(policy);

            var newVersionPolicy = new LeavePolicy
            {
                Id = Guid.NewGuid(),
                LeaveType = policy.LeaveType,
                AnnualAllowance = request.AnnualAllowance,
                MaxConsecutiveDays = request.MaxConsecutiveDays,
                MinNoticeDays = request.MinNoticeDays,
                BackdateDays = request.BackdateDays,
                IsEnabled = true,
                Version = policy.Version + 1
            };

            await _unitOfWork.LeavePolicies.AddAsync(newVersionPolicy);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok(MapToResponse(newVersionPolicy));
        }
        else
        {
            policy.AnnualAllowance = request.AnnualAllowance;
            policy.MaxConsecutiveDays = request.MaxConsecutiveDays;
            policy.MinNoticeDays = request.MinNoticeDays;
            policy.BackdateDays = request.BackdateDays;

            _unitOfWork.LeavePolicies.Update(policy);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok(MapToResponse(policy));
        }
    }

    public async Task<Result<LeavePolicyResponse>> ToggleStatusAsync(ToggleStatusRequest request)
    {
        var policy = await _unitOfWork.LeavePolicies.GetByIdAsync(request.Id);
        if (policy == null)
            return Result.Fail<LeavePolicyResponse>(new NotFoundError("Policy not found."));
        if (!request.IsEnabled && await _unitOfWork.LeavePolicies.HasBeenUsedAsync(request.Id))
            return Result.Fail<LeavePolicyResponse>(new BadRequestError("Cannot disable a policy that has been used."));
        var existingActive = await _unitOfWork.LeavePolicies.GetActiveByTypeAsync(policy.LeaveType);
        if (request.IsEnabled && !policy.IsEnabled && existingActive != null && existingActive.Id != policy.Id)
            return Result.Fail<LeavePolicyResponse>(new BadRequestError("Cannot enable this policy because another active policy of the same type exists."));


        policy.IsEnabled = request.IsEnabled;
        _unitOfWork.LeavePolicies.Update(policy);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(policy));
    }

    private LeavePolicyResponse MapToResponse(LeavePolicy policy) => new()
    {
        Id = policy.Id,
        LeaveType = policy.LeaveType.ToString(),
        IsEnabled = policy.IsEnabled,
        AnnualAllowance = policy.AnnualAllowance,
        MaxConsecutiveDays = policy.MaxConsecutiveDays,
        MinNoticeDays = policy.MinNoticeDays,
        BackdateDays = policy.BackdateDays,
        Version = policy.Version
    };
}