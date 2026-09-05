namespace HrSystem.Application.Features.EmployeeParticipation.DTO.ResponseDto;


public class ParticipationStatusResponse
{
    public bool IsOptedIn { get; set; }
    public DateTime? LastOptOutDate { get; set; }
    public DateTime? CooldownEndDate { get; set; }
}
