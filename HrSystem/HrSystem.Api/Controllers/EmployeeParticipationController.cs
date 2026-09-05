using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HrSystem.Application.Features.EmployeeParticipation.IService;
using HrSystem.Application.Extensions;
using HrSystem.Application.Features.EmployeeParticipation.DTO.RequestDto;
namespace HrSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeParticipationController : ControllerBase
    {
        private readonly IEmployeeParticipationService _participationService;

        public EmployeeParticipationController(IEmployeeParticipationService participationService)
        {
            _participationService = participationService;
        }

  

         private IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult> onSuccess)
        {
            if (result.IsFailed)
            {
                return result.ToActionResult();
            }
            return onSuccess(result.Value);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetMyStatus()
        {
            var result = await _participationService.GetMyStatusAsync();
            return HandleResult(result, response => Ok(response));
        }
        [HttpGet("status/{id:guid}")]
        public async Task<IActionResult> GetStatusById(Guid id)
        {
            var result = await _participationService.GetStatusByIdAsync(id);
            return HandleResult(result, response => Ok(response));
        }

        [HttpPost("opt-in")]
        public async Task<IActionResult> OptIn()
        {
            var result = await _participationService.OptInAsync();
            return HandleResult(result, value => Ok(value));
        }

        [HttpPost("opt-out")]
        public async Task<IActionResult> OptOut()
        {
            var result = await _participationService.OptOutAsync();
            return HandleResult(result, value => Ok(value));
        }

        [HttpPut("admin/policy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGlobalPolicy([FromBody] UpdateGlobalPolicyRequest dto)
        {
            var result = await _participationService.UpdateGlobalPolicyAsync(dto);
            return HandleResult(result, value => Ok(value));
        }

        [HttpPatch("admin/employees/{employeeId:guid}/force-participation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ForceChangeParticipation(Guid employeeId, [FromBody] ForceParticipationRequest dto)
        {
            var result = await _participationService.ForceChangeEmployeeParticipationAsync(employeeId, dto);
            return HandleResult(result, value => Ok(value));
        }
    }
}