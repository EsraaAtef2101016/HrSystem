using FluentValidation;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;
namespace HrSystem.Application.Features.PublicHolidayFeature.Validator;
    public class CreatePublicHolidayRequestValidator : AbstractValidator<CreatePublicHolidayRequest>
    {
        public CreatePublicHolidayRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Holiday date is required.") .WithErrorCode("HOLIDAY_DATE_REQUIRED")
                .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Holidays can only be managed for future dates.") .WithErrorCode("HOLIDAY_FUTURE_DATE_REQUIRED");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Holiday name is required.") .WithErrorCode("HOLIDAY_NAME_REQUIRED")
                .MaximumLength(100).WithMessage("Holiday name cannot exceed 100 characters.").WithErrorCode("HOLIDAY_NAME_TOO_LONG");
        }

    }
