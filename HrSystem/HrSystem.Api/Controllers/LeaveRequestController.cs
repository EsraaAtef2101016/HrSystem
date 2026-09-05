using FluentResults;
using HrSystem.Application.Extensions;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;
using HrSystem.Application.Features.LeaveRequestFeature.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _leaveRequestService.GetAllAsync();
            return HandleResult(result, value => Ok(value));
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var result = await _leaveRequestService.GetMyRequestsAsync();
            return HandleResult(result, value => Ok(value));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _leaveRequestService.GetByIdAsync(id);
            return HandleResult(result, value => Ok(value));
        }

        [HttpPost("create")]
        [Authorize(Roles = "Employee,Manager")]

        public async Task<IActionResult> Create([FromBody] CreateLeaveRequestRequest request)
        {
            var result = await _leaveRequestService.CreateAsync(request);
            return HandleResult(result, value => CreatedAtAction(nameof(GetById), new { id = value.Id }, value));
        }

        [HttpPut("update/{id:guid}")]
        [Authorize(Roles = "Employee,Manager")]

        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveRequestRequest request)
        {
            var result = await _leaveRequestService.UpdateAsync(id, request);
            return HandleResult(result, value => Ok(value));
        }

        [HttpPatch("cancel/{id:guid}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _leaveRequestService.CancelAsync(id);
            return HandleResult(result, value => Ok(value));
        }

        [HttpGet("balances/current")]
        public async Task<IActionResult> GetMyCurrentYearBalances()
        {
            var result = await _leaveRequestService.GetMyBalancesAsync();
            return HandleResult(result, value => Ok(value));

        }
        private IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult> onSuccess)
        {
            if (result.IsFailed) return result.ToActionResult();
            return onSuccess(result.Value);
        }
    }
}