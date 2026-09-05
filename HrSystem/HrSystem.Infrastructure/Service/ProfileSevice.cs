using FluentResults;
using HrSystem.Application.Common.DTO.ErrorDto;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.ResponseDto;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Application.Extensions;
using HrSystem.Application.Features.ProfileFeature.DTO.RequestDto;
using HrSystem.Application.Features.ProfileFeature.DTO.ResponseDto;
using HrSystem.Application.Features.ProfileFeature.IService;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;

namespace HrSystem.Infrastructure.Service;


public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ProfileService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<Result<UserProfileResponse>> GetUserProfileAsync(Guid id)
    {
        var formatValidation = await UserIdentityValidation(id);
        if (formatValidation.IsFailed)
            return Result.Fail<UserProfileResponse>(formatValidation.Errors);

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return Result.Fail<UserProfileResponse>(new NotFoundError("not Found user."));

        return Result.Ok(MapToResponse(user));
    }

    public async Task<Result<UserProfileResponse>> UserIdentityValidation(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return Result.Fail<UserProfileResponse>(new NotFoundError("Manager not found."));

        return Result.Ok<UserProfileResponse>(default!);
    }

    public async Task<Result<UserProfileResponse>> GetMyProfileAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Result.Fail<UserProfileResponse>(new UnauthorizedError("Unauthorized user."));

        return await GetUserProfileAsync(userId.Value);
    }
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }


    public async Task<Result<IEnumerable<UserProfileResponse>>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var filteredUsers = users.Where(user => user.Role != UserRole.Admin);

        var response = filteredUsers.Select(user => MapToResponse(user));

        return Result.Ok(response);
    }
    public async Task<Result<IEnumerable<UserProfileResponse>>> GetAllAdminsAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
      var filteredUsers = users.Where(user => user.Role == UserRole.Admin);

         var response = filteredUsers.Select(user => MapToResponse(user));

        return Result.Ok(response);
    }

    public async Task<Result<UserProfileResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request)
    {
        var identityValidation = await UserIdentityValidation(request.Id);
        if (identityValidation.IsFailed)
            return Result.Fail<UserProfileResponse>(identityValidation.Errors);

        var enumValidationResult = ValidationExtensions.ValidateEnum<UserRole, UserProfileResponse>(request.UserRole, nameof(request.UserRole));
        if (enumValidationResult.IsFailed)
        {
            return Result.Fail<UserProfileResponse>(enumValidationResult.Errors);
        }
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null)
            return Result.Fail<UserProfileResponse>(new NotFoundError("not Found user."));

        if (request.UserRole == null)
        {
            return Result.Ok(MapToResponse(user));
        }
        Enum.TryParse<UserRole>(request.UserRole, ignoreCase: true, out var parsedRole);
        user.Role = parsedRole;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(user));
    }

    public async Task<Result<UserProfileResponse>> UpdateUserStatusAsync(UpdateUserStatusRequest request)
    {
        var identityValidation = await UserIdentityValidation(request.Id);
        if (identityValidation.IsFailed)
            return Result.Fail<UserProfileResponse>(identityValidation.Errors);

        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
       
        if (user == null)
            return Result.Fail<UserProfileResponse>(new NotFoundError("not Found user."));

        user.IsActive = request.IsActive;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(user));
    }

    public async Task<Result<UserProfileResponse>> UpdateUserManagerAsync(UpdateUserManagerRequest request)
    {
        var identityValidation = await UserIdentityValidation(request.Id);
        if (identityValidation.IsFailed)
            return Result.Fail<UserProfileResponse>(identityValidation.Errors);

        if (request.ManagerId.HasValue)
        {
            var manager = await _unitOfWork.Users.GetByIdAsync(request.ManagerId.Value);

            if (manager == null || manager.Role != UserRole.Manager)
            {
                return Result.Fail<UserProfileResponse>(new NotFoundError("Manager not found."));
            }
        }

        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        user!.ManagerId = request.ManagerId;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(MapToResponse(user));
    }

    private UserProfileResponse MapToResponse(User user) => new()
    {
        Id = user.Id,
        Name = user.Name ?? string.Empty,
        Email = user.Email ?? string.Empty,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        ManagerId = user.ManagerId
    };
}

