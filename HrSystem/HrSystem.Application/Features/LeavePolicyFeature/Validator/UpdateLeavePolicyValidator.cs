using FluentValidation;
using HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;

namespace HrSystem.Application.Features.LeavePolicyFeature.Validators;

public class UpdateLeavePolicyValidator : AbstractValidator<UpdateLeavePolicyRequest>
{
    public UpdateLeavePolicyValidator()
    {
        RuleFor(x => x.AnnualAllowance)
            .GreaterThan(0).WithMessage("Annual allowance must be greater than zero.").WithErrorCode("AnnualAllowanceInvalid");

        RuleFor(x => x.MaxConsecutiveDays)
            .GreaterThan(0).WithMessage("Max consecutive days must be greater than zero.").WithErrorCode("MaxConsecutiveDaysInvalid");

        RuleFor(x => x.MinNoticeDays)
            .GreaterThanOrEqualTo(0).WithMessage("Min notice days cannot be negative.").WithErrorCode("MinNoticeDaysInvalid");

        RuleFor(x => x.BackdateDays)
            .GreaterThanOrEqualTo(0).WithMessage("Backdate days cannot be negative.").WithErrorCode("BackdateDaysInvalid");
    }
}
