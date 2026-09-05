using FluentResults;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.ResponseDto;

namespace HrSystem.Application.Features.PublicHolidayFeature.IService;

public interface IPublicHolidayService
{
    Task<Result<IEnumerable<PublicHolidayResponse>>> GetAllFutureHolidaysAsync();
    Task<Result<PublicHolidayResponse>> GetByIdAsync(Guid id);
    Task<Result<PublicHolidayResponse>> CreateAsync(CreatePublicHolidayRequest request);
    Task<Result<PublicHolidayResponse>> UpdateAsync(Guid id, UpdatePublicHolidayRequest request);
    Task<Result<MessageResponse>> DeleteAsync(Guid id);
    Task<Result<IEnumerable<PublicHolidayResponse>>> GetAllHolidaysAsync();
}