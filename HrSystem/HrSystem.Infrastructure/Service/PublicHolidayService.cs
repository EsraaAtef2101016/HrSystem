using FluentResults;
using FluentValidation;
using HrSystem.Application.Common.DTO.ErrorDto;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Application.Extensions;
using HrSystem.Domain.Entities;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using HrSystem.Application.Features.PublicHolidayFeature.IService;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.ResponseDto;
namespace HrSystem.Infrastructure.Service
{
    public class PublicHolidayService : IPublicHolidayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreatePublicHolidayRequest> _createValidator;
        private readonly IValidator<UpdatePublicHolidayRequest> _updateValidator;

        public PublicHolidayService(
            IUnitOfWork unitOfWork,
            IValidator<CreatePublicHolidayRequest> createValidator,
            IValidator<UpdatePublicHolidayRequest> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<IEnumerable<PublicHolidayResponse>>> GetAllFutureHolidaysAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var holidays = await _unitOfWork.PublicHolidays.GetFutureHolidaysAsync(today);

            var response = holidays.Select(h => new PublicHolidayResponse
            {
                Id = h.Id,
                Date = h.Date,
                Name = h.Name
            });

            return Result.Ok(response);
        }

          public async Task<Result<IEnumerable<PublicHolidayResponse>>> GetAllHolidaysAsync()
        {


            var holidays = await _unitOfWork.PublicHolidays.GetAllHolidaysAsync();

            var response = holidays.Select(h => new PublicHolidayResponse
            {
                Id = h.Id,
                Date = h.Date,
                Name = h.Name
            });

            return Result.Ok(response);
        }

        public async Task<Result<PublicHolidayResponse>> GetByIdAsync(Guid id)
        {
            var validationResult = await HolidayIdValidation(id);
            if (validationResult.IsFailed)
                return Result.Fail<PublicHolidayResponse>(validationResult.Errors);

            var holiday = await _unitOfWork.PublicHolidays.GetByIdAsync(id);

            if(holiday ==null)
                return Result.Fail<PublicHolidayResponse>(new NotFoundError("not Found holidays."));
       
            return Result.Ok(new PublicHolidayResponse
            {
                Id = holiday.Id,
                Date = holiday.Date,
                Name = holiday.Name
            });
        }

        public async Task<Result<PublicHolidayResponse>> CreateAsync(CreatePublicHolidayRequest request)
        {
            var validationResult = await CreateHolidayValidation(request);
            if (validationResult.IsFailed)
                return Result.Fail<PublicHolidayResponse>(validationResult.Errors);

            var holiday = PublicHoliday.Create(request.Date, request.Name);

            await _unitOfWork.PublicHolidays.AddAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok(new PublicHolidayResponse
            {
                Id = holiday.Id,
                Date = holiday.Date,
                Name = holiday.Name
            });
        }

        public async Task<Result<PublicHolidayResponse>> CreateHolidayValidation(CreatePublicHolidayRequest request)
        {
            var validationResult = await _createValidator.ValidateRequestAsync<CreatePublicHolidayRequest, PublicHolidayResponse>(request, StatusCodes.Status400BadRequest);
            if (validationResult.IsFailed)
            {
                return Result.Fail<PublicHolidayResponse>(validationResult.Errors);
            }
            if (request.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                return Result.Fail<PublicHolidayResponse>(new BadRequestError("Cannot add past holidays."));
            }
            return Result.Ok<PublicHolidayResponse>(default!);
        }

        public async Task<Result<PublicHolidayResponse>> UpdateAsync(Guid id, UpdatePublicHolidayRequest request)
        {
            var validationResult = await UpdateHolidayValidation(id, request);
            if (validationResult.IsFailed)
                return Result.Fail<PublicHolidayResponse>(validationResult.Errors);

            var holiday = await _unitOfWork.PublicHolidays.GetByIdAsync(id);
            if(holiday ==null)
                return Result.Fail<PublicHolidayResponse>(new NotFoundError("not Found holidays."));
       
            holiday.UpdateDetails(request.Date, request.Name);

            _unitOfWork.PublicHolidays.Update(holiday);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok(new PublicHolidayResponse
            {
                Id = holiday.Id,
                Date = holiday.Date,
                Name = holiday.Name
            });
        }

        public async Task<Result<PublicHolidayResponse>> UpdateHolidayValidation(Guid id, UpdatePublicHolidayRequest request)
        {
            var validationResult = await _updateValidator.ValidateRequestAsync<UpdatePublicHolidayRequest, PublicHolidayResponse>(request, StatusCodes.Status400BadRequest);
            if (validationResult.IsFailed)
            {
                return Result.Fail<PublicHolidayResponse>(validationResult.Errors);
            }
            if (request.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                var error = new Error("Cannot update to a past date.")
                    .WithMetadata("Code", "holiday.past_date")
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest);
                return Result.Fail<PublicHolidayResponse>(error);
            }
            var holidayIdValidationResult = await HolidayIdValidation(id);
            if (holidayIdValidationResult.IsFailed)
                return Result.Fail<PublicHolidayResponse>(holidayIdValidationResult.Errors);

            return Result.Ok<PublicHolidayResponse>(default!);
        }

        public async Task<Result<MessageResponse>> DeleteAsync(Guid id)
        {
            var validationResult = await DeleteValidation(id);
            if (validationResult.IsFailed)
                return Result.Fail<MessageResponse>(validationResult.Errors);

            var holiday = await _unitOfWork.PublicHolidays.GetByIdAsync(id);
            if(holiday ==null)
                return Result.Fail<MessageResponse>(new NotFoundError("not Found holidays."));
       
            _unitOfWork.PublicHolidays.Remove(holiday);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok(new MessageResponse
            {
                Status = "Success",
                Message = "Deleted successfully."
            });
        }

        public async Task<Result<MessageResponse>> DeleteValidation(Guid id)
        {
            var validationResult = await HolidayIdValidation(id);
            if (validationResult.IsFailed)
                return Result.Fail<MessageResponse>(validationResult.Errors);

            var holiday = await _unitOfWork.PublicHolidays.GetByIdAsync(id);
            if (holiday == null)
                return Result.Fail<MessageResponse>(new NotFoundError("Holiday not found."));

            if (holiday.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                return Result.Fail<MessageResponse>(new BadRequestError("Cannot delete past holidays."));
            }

            return Result.Ok(new MessageResponse
            {
                Status = "Success",
                Message = "Validation passed."
            });
        }

        public async Task<Result<PublicHolidayResponse>> HolidayIdValidation(Guid id)
        {
            var holiday = await _unitOfWork.PublicHolidays.GetByIdAsync(id);
            if (holiday == null)
            {
                var error = new Error("Public holiday not found.")
                    .WithMetadata("Code", "holiday.not_found")
                    .WithMetadata("StatusCode", StatusCodes.Status404NotFound);

                return Result.Fail<PublicHolidayResponse>(error);
            }

            return Result.Ok<PublicHolidayResponse>(default!);
        }
    }
}