using FluentValidation;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;

namespace HrSystem.Application.Features.LeaveRequestFeature.Validator;

public class UpdateLeaveRequestValidator : AbstractValidator<UpdateLeaveRequestRequest>
{
    public UpdateLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveType)
            .IsInEnum()
            .WithMessage("Invalid leave type.")
            .WithErrorCode("InvalidLeaveType")
            ;

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.")
            .WithErrorCode("StartDateRequired");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date.")
            .WithErrorCode("EndDateInvalid");
    }
}