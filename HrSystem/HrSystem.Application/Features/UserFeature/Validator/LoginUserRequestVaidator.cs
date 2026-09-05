using FluentValidation;
 
using HrSystem.Application.Features.UserFeature.DTO.RequestDto;

 namespace HrSystem.Application.Features.UserFeature.Validator;
 public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
    
     RuleFor(x => x.Email)
      .NotEmpty().WithMessage("A valid email address is required.").NotNull().WithMessage("A valid email address is required.")
      .WithErrorCode("INVALID_CREDENTIALS")
      .EmailAddress().WithMessage("Email address is not valid.")
      .WithErrorCode("INVALID_CREDENTIALS")
      ;

     RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password is required.").NotNull().WithMessage("Password is required.")
      .WithErrorCode("INVALID_CREDENTIALS");
    }
}