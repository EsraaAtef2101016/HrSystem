using FluentResults;
using HrSystem.Application.Common.DTO.ResponseDto;
using HrSystem.Application.Features.EmployeeParticipation.DTO.RequestDto;
using HrSystem.Application.Features.EmployeeParticipation.DTO.ResponseDto;
namespace HrSystem.Application.Features.EmployeeParticipation.IService;

public interface IEmployeeParticipationService
{
    Task<Result<ParticipationStatusResponse>> GetMyStatusAsync();
    Task<Result<ParticipationStatusResponse>> GetStatusByIdAsync(Guid userId);
    Task<Result<MessageResponse>> OptOutAsync();
     Task<Result<MessageResponse>> OptInAsync();
     Task<Result<MessageResponse>> UpdateGlobalPolicyAsync(UpdateGlobalPolicyRequest dto);
     Task<Result<MessageResponse>> ForceChangeEmployeeParticipationAsync(Guid employeeId, ForceParticipationRequest dto);
}