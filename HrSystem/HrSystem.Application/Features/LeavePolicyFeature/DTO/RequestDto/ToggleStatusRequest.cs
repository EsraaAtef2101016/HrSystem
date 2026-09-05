namespace HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;

public class ToggleStatusRequest
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
}
