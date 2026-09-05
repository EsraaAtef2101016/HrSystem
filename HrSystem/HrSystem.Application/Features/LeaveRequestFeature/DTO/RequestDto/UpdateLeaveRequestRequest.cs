using HrSystem.Domain.Enums;
namespace HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;


public class UpdateLeaveRequestRequest
{
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
