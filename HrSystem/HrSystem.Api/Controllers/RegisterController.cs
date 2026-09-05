using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using HrSystem.Application.Extensions;                 // ToActionResult extension method

using HrSystem.Application.Features.UserFeature.DTO.RequestDto;
using HrSystem.Application.Features.UserFeature.IService;
namespace HrSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<RegisterUserRequest> _validator;
      
        public RegisterController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequest request)
        {
            var result = await _userService.RegisterAsync(request);

            return HandleResult(result, value => Created(string.Empty, value));
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
}
