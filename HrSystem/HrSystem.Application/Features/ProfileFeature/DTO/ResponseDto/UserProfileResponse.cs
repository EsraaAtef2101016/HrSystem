namespace HrSystem.Application.Features.ProfileFeature.DTO.ResponseDto;

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public  string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public Guid? ManagerId { get; set; } 
}
