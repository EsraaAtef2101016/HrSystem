using FluentValidation;

using HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.LeavePolicyFeature.Validators;


public class CreateLeavePolicyValidator : AbstractValidator<CreateLeavePolicyRequest>
{
    public CreateLeavePolicyValidator()
    {
        RuleFor(x => x.LeaveType)
            .Must(leaveType => Enum.TryParse<LeaveType>(leaveType, true, out _))
            .WithMessage("Invalid leave type specified.");
        RuleFor(x => x.AnnualAllowance)
            .GreaterThan(0).WithMessage("Annual allowance must be greater than zero.");

        RuleFor(x => x.MaxConsecutiveDays)
            .GreaterThan(0).WithMessage("Max consecutive days must be greater than zero.");

        RuleFor(x => x.MinNoticeDays)
            .GreaterThanOrEqualTo(0).WithMessage("Min notice days cannot be negative.");

        RuleFor(x => x.BackdateDays)
            .GreaterThanOrEqualTo(0).WithMessage("Backdate days cannot be negative.");
    }
}
