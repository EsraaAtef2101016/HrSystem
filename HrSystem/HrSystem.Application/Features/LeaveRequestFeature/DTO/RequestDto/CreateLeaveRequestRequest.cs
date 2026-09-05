using HrSystem.Domain.Enums;
//C:\Users\DELL\source\repos\HrSystem\HrSystem.Application\Features\LeaveRequestFeature\DTO\RequestDto
namespace HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;

public class CreateLeaveRequestRequest
{
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}


