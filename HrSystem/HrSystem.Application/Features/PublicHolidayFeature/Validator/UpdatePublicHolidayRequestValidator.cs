using FluentValidation;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;
namespace HrSystem.Application.Features.PublicHolidayFeature.Validator;

public class UpdatePublicHolidayRequestValidator : AbstractValidator<UpdatePublicHolidayRequest>
{
    public UpdatePublicHolidayRequestValidator()
    {
        RuleFor(x => x.Date)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Holidays can only be managed for future dates.").WithErrorCode("HOLIDAY_FUTURE_DATE_REQUIRED");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Holiday name cannot exceed 100 characters.").WithErrorCode("HOLIDAY_NAME_TOO_LONG");
    }
}
