namespace HrSystem.Application.Features.UserFeature.DTO.ResponseDto;

public class RegisterUserResponse
{
    public Guid userId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}