namespace HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;


public class UpdateLeavePolicyRequest
{
    public Guid id { get; set; }
    public int AnnualAllowance { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int MinNoticeDays { get; set; }
    public int BackdateDays { get; set; }
}
