using FluentValidation;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;
using System.ComponentModel.DataAnnotations;
//using HrSystem.Application.Features.LeaveRequestFeature.Validator;

namespace HrSystem.Application.Features.LeaveRequestFeature.Validator;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestRequest>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveType).IsInEnum().WithMessage("Invalid leave type.");
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Start date cannot be in the past.");
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}
