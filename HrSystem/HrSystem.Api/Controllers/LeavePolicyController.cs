using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HrSystem.Application.Extensions;
using HrSystem.Application.Features.LeavePolicyFeature.IService;
using HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;
namespace HrSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class LeavePolicyController : ControllerBase
{
    private readonly ILeavePolicyService _leavePolicyService;

    public LeavePolicyController(ILeavePolicyService leavePolicyService)
    {
        _leavePolicyService = leavePolicyService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _leavePolicyService.GetAllAsync();
        return HandleResult(result, value => Ok(value));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _leavePolicyService.GetByIdAsync(id);
        return HandleResult(result, value => Ok(value));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateLeavePolicyRequest request)
    {
        var result = await _leavePolicyService.CreateAsync(request);
        return HandleResult(result, value => CreatedAtAction(nameof(GetById), new { id = value.Id }, value));
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update( [FromBody] UpdateLeavePolicyRequest request)
    {
        var result = await _leavePolicyService.UpdateAsync(request);
        return HandleResult(result, value => Ok(value));
    }

    [HttpPatch("status")]
    public async Task<IActionResult> ToggleStatus(ToggleStatusRequest request)
    {
        var result = await _leavePolicyService.ToggleStatusAsync(request);
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
