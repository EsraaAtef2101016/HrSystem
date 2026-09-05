using FluentResults;
using FluentValidation;
using HrSystem.Application.Extensions;                 // ToActionResult extension method
using HrSystem.Application.Features.UserFeature.DTO.RequestDto;
using HrSystem.Application.Features.UserFeature.IService;
using HrSystem.Infrastructure.Persistence.Context;      // ApplicationDBContext
using Microsoft.AspNetCore.Mvc;
namespace HrSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<LoginUserRequest> _validator;
    private readonly ApplicationDBContext context;
    public LoginController(IValidator<LoginUserRequest> validator, ApplicationDBContext _context, IUserService userService)
    {
        _validator = validator;
        context = _context;
        _userService = userService;
    }




    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginUserRequest request)
    {
        var result = await _userService.LoginAsync(request);

        return HandleResult(result, value => Ok(value));
    }



    private IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.IsFailed)
        {
            return result.ToActionResult();
        }
        return onSuccess(result.Value);
    }



}
