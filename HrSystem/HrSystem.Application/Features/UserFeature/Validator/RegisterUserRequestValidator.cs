using FluentValidation;
using HrSystem.Domain.Enums;

using HrSystem.Application.Features.UserFeature.DTO.RequestDto;

namespace HrSystem.Application.Validation
{
    public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.").WithErrorCode("EMAIL_REQUIRED")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .WithErrorCode("PASSWORD_REQUIRED")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .WithErrorCode("PASSWORD_TOO_SHORT")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .WithErrorCode("PASSWORD_MISSING_UPPERCASE")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .WithErrorCode("PASSWORD_MISSING_NUMBER");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Display name is required.").WithErrorCode("DISPLAY_NAME_REQUIRED");

            RuleFor(x => x.UserRole)
                .NotEmpty().WithMessage("User role is required.").WithErrorCode("USER_ROLE_REQUIRED")
                .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
                .WithMessage("Invalid user role.").WithErrorCode("INVALID_USER_ROLE");
        }
    }
}