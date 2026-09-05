namespace HrSystem.Application.Features.UserFeature.DTO.ResponseDto;
public class LoginUserResponse
{
    public string? accessToken { get; set; }
    public DateTime expiresAtUtc { get; set; }
    public UserModel user { get; set; }

}
public class UserModel
{ 
    public Guid userId { get; set; }
    public string? email { get; set; }
    public string? displayName { get; set; }
    public string? role { get; set; }

}