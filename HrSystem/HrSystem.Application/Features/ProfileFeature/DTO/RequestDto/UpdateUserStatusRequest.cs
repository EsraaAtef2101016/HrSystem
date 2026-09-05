namespace HrSystem.Application.Features.ProfileFeature.DTO.RequestDto;


public class UpdateUserStatusRequest
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}


