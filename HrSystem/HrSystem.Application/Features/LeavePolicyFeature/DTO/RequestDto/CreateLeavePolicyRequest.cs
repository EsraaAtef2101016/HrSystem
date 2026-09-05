using HrSystem.Domain.Enums;
using static System.Net.Mime.MediaTypeNames;
namespace HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;

public class CreateLeavePolicyRequest
{
    public string LeaveType { get; set; }
    public int AnnualAllowance { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int MinNoticeDays { get; set; }
    public int BackdateDays { get; set; }
}
