using System;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using HrSystem.Application.Extensions;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using HrSystem.Infrastructure.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using HrSystem.Application.Features.UserFeature.DTO.RequestDto;
using HrSystem.Application.Features.UserFeature.DTO.ResponseDto;
using HrSystem.Application.Features.UserFeature.IService;
using HrSystem.Application.Features.UserFeature.Validator;
namespace HrSystem.Infrastructure.Service
{
    public class UserService : IUserService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IValidator<RegisterUserRequest> _registerUserValidator;
        private readonly JwtOptions _jwtOptions;
        private readonly IValidator<LoginUserRequest> _loginUserValidator;

        public UserService(
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IPasswordHasher<User> passwordHasher,
            IOptions<JwtOptions> jwtOptions,
            IValidator<RegisterUserRequest> registerUserValidator,
            IValidator<LoginUserRequest> loginUserValidator)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
            _registerUserValidator = registerUserValidator;
            _loginUserValidator = loginUserValidator;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<Result<RegisterUserResponse>> RegisterAsync(RegisterUserRequest request)
        {
            var formatValidation = await RegisterValidation(request);
            if (formatValidation.IsFailed)
                return Result.Fail<RegisterUserResponse>(formatValidation.Errors);

            Enum.TryParse<UserRole>(request.UserRole, ignoreCase: true, out var parsedRole);

            var user = User.Create(request.Name, request.Email, parsedRole, request.ManagerId);


            var hashedPassword = _passwordHasher.HashPassword(user, request.Password);
            user.SetPasswordHash(hashedPassword);

            await _unitOfWork.Users.AddAsync(user);
            await InitializeLeaveBalancesForNewUserAsync(user.Id);
            await _unitOfWork.SaveChangesAsync();


            var response = new RegisterUserResponse
            {
                userId = user.Id,
                Email = user.Email,
                DisplayName = user.Name,
                UserRole = user.Role.ToString(),
                Message = "User registered successfully."
            };
            return Result.Ok(response);
        }



        public async Task<Result<RegisterUserResponse>> RegisterValidation(RegisterUserRequest request)
        {
            var validationResult = await _registerUserValidator.ValidateRequestAsync<RegisterUserRequest, RegisterUserResponse>(request, StatusCodes.Status400BadRequest);
            if (validationResult.IsFailed)
            {
                return Result.Fail<RegisterUserResponse>(validationResult.Errors);
            }

            var enumValidationResult = ValidationExtensions.ValidateEnum<UserRole, RegisterUserResponse>(request.UserRole, nameof(request.UserRole));
            if (enumValidationResult.IsFailed)
            {
                return Result.Fail<RegisterUserResponse>(enumValidationResult.Errors);
            }

            if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email))
            {
                var error = new Error("The email is already registered.")
                    .WithMetadata("Code", "auth.email_in_use")
                    .WithMetadata("PropertyName", nameof(request.Email))
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest);

                return Result.Fail<RegisterUserResponse>(error);
            }

            return Result.Ok<RegisterUserResponse>(default!);
        }

        private async Task InitializeLeaveBalancesForNewUserAsync(Guid employeeId)
        {
            int currentYear = DateTime.UtcNow.Year;
            var allPolicies = await _unitOfWork.LeavePolicies.GetAllAsync();
            var activePolicies = allPolicies.Where(p => p.IsEnabled);
            foreach (var policy in activePolicies)
            {
                var leaveBalance = new LeaveBalance
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    LeaveType = policy.LeaveType,
                    Year = currentYear,
                    InitialAllowance = policy.AnnualAllowance,
                    UsedDays = 0,
                    ReservedDays = 0
                };

                await _unitOfWork.LeaveBalances.AddAsync(leaveBalance);
            }
        }


        public async Task<Result<LoginUserResponse>> LoginAsync(LoginUserRequest request)
        {
            var formatValidation = await LoginValidation(request);
            if (formatValidation.IsFailed)
                return Result.Fail<LoginUserResponse>(formatValidation.Errors);

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            int lifetimeInSeconds = _jwtOptions.Lifetime > 0 ? _jwtOptions.Lifetime : 3600;
            var tokenExpiresAt = DateTime.UtcNow.AddSeconds(lifetimeInSeconds);

            var tokenString = _jwtTokenGenerator.GenerateToken(user);

            var response = new LoginUserResponse
            {
                accessToken = tokenString,
                expiresAtUtc = tokenExpiresAt,
                user = new UserModel
                {
                    userId = user.Id,
                    email = user.Email,
                    displayName = user.Name,
                    role = user.Role.ToString()
                }
            };

            return Result.Ok(response);
        }

        public async Task<Result<LoginUserResponse>> LoginValidation(LoginUserRequest request)
        {
            var validationResult = await _loginUserValidator.ValidateRequestAsync<LoginUserRequest, LoginUserResponse>(request, StatusCodes.Status400BadRequest);
            if (validationResult.IsFailed)
            {
                return Result.Fail<LoginUserResponse>(validationResult.Errors);
            }

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
            {
                var error = new Error("Invalid email or password.")
                    .WithMetadata("Code", "auth.invalid_credentials")
                    .WithMetadata("PropertyName", nameof(request.Email))
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest);

                return Result.Fail<LoginUserResponse>(error);
            }
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                var error = new Error("Invalid email or password.")
                    .WithMetadata("Code", "auth.invalid_credentials")
                    .WithMetadata("PropertyName", nameof(request.Password))
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest);

                return Result.Fail<LoginUserResponse>(error);
            }

            return Result.Ok<LoginUserResponse>(default!);
        }
    }
}