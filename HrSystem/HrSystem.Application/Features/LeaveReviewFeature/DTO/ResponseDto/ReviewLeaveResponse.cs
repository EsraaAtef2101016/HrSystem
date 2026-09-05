
namespace HrSystem.Application.Features.LeaveReviewFeature.DTO.ResponseDto;
public class ReviewLeaveResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}