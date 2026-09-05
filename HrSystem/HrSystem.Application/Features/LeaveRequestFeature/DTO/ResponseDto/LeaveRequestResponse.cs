namespace HrSystem.Application.Features.LeaveRequestFeature.DTO.ResponseDto;

public class LeaveRequestResponse
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ChargedDays { get; set; }
    public string? RejectionReason { get; set; }
    public int PolicyVersionSnapshot { get; set; }
    public decimal PolicyAllowanceSnapshot { get; set; }
    public DateTime CreatedAt { get; set; }
}
