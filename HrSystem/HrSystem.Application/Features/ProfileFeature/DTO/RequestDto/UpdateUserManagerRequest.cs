namespace HrSystem.Application.Features.ProfileFeature.DTO.RequestDto;

public class UpdateUserManagerRequest
{
    public Guid Id { get; set; }
    public Guid? ManagerId { get; set; }
}
