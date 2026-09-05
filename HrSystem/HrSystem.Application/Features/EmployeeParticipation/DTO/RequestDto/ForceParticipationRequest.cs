using System.ComponentModel.DataAnnotations;

namespace HrSystem.Application.Features.EmployeeParticipation.DTO.RequestDto;

public class ForceParticipationRequest
{
    public bool ForceOptIn { get; set; }

    [Required(ErrorMessage = "A reason is required for forced participation changes.")]
    public string Reason { get; set; } = string.Empty;
}
