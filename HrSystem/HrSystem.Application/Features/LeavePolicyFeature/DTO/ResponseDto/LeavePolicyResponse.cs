namespace HrSystem.Application.Features.LeavePolicyFeature.DTO.ResponseDto;
public class LeavePolicyResponse
{
    public Guid Id { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int AnnualAllowance { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int MinNoticeDays { get; set; }
    public int BackdateDays { get; set; }
    public int Version { get; set; }
}
