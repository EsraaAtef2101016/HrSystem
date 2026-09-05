using FluentResults;
using FluentValidation;
using HrSystem.Application.Common.Exceptions;
using HrSystem.Application.Extensions;
using HrSystem.Application.Features.ProfileFeature.DTO.RequestDto;
using HrSystem.Application.Features.ProfileFeature.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace HrSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
    

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _profileService.GetUserProfileAsync(id);

            return HandleResult(result, value => Ok(value));
        }

        [Authorize]
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _profileService.GetMyProfileAsync();
            return HandleResult(result, value => Ok(value));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _profileService.GetAllUsersAsync();
            return HandleResult(result, value => Ok(value));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("all/Admin")]
        public async Task<IActionResult> GetAllAdminUsers()
        {
            var result = await _profileService.GetAllAdminsAsync();
            return HandleResult(result, value => Ok(value));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
        {
            var result = await _profileService.UpdateUserRoleAsync(request);
            return HandleResult(result, value => Ok(value));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusRequest request)
        {
            var result = await _profileService.UpdateUserStatusAsync(request);
            return HandleResult(result, value => Ok(value));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("manager")]
        public async Task<IActionResult> UpdateUserManager([FromBody] UpdateUserManagerRequest request)
        {
            var result = await _profileService.UpdateUserManagerAsync(request);
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
}