using FluentResults;
using HrSystem.Application.Extensions;
using HrSystem.Application.Features.LeaveReviewFeature.IService;
using HrSystem.Application.Features.LeaveReviewFeature.DTO.RequestDto;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HrSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LeaveReviewController : ControllerBase
{
    private readonly ILeaveReviewService _leaveReviewService;

    public LeaveReviewController(ILeaveReviewService leaveReviewService)
    {
        _leaveReviewService = leaveReviewService;
    }




    private IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.IsFailed)
        {
            return result.ToActionResult();
        }
        return onSuccess(result.Value);
    }


    [HttpPatch("{id:guid}/accept")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var result = await _leaveReviewService.AcceptAsync(id);
        return HandleResult(result, response => Ok(response));
    }

    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewLeaveRequest request)
    {
        var result = await _leaveReviewService.RejectAsync(id, request.RejectionReason);
        return HandleResult(result, response => Ok(response));
    }

    [HttpGet("manager/pending")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetManagerPendingRequests()
    {
        var result = await _leaveReviewService.GetManagerPendingRequestsAsync();
        return HandleResult(result, response => Ok(response));
    }

    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPendingRequestsForAdmin()
    {
        var result = await _leaveReviewService.GetAllPendingRequestsForAdminAsync();
        return HandleResult(result, response => Ok(response));
    }
}
