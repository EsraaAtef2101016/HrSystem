namespace HrSystem.Application.Features.LeaveRequestFeature.DTO.ResponseDto;

    public class LeaveBalanceSummaryResponse
{
    public string LeaveType { get; set; } = string.Empty;
    public int InitialAllowance { get; set; }
    public int UsedDays { get; set; }
    public int ReservedDays { get; set; }
    public int AvailableDays { get; set; }
    public int Year { get; set; }
}