using FluentResults;
using HrSystem.Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HrSystem.Application.Features.PublicHolidayFeature.IService;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;
namespace HrSystem.Api.Controllers
{
    [Route("api/public-holidays")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PublicHolidayController : ControllerBase
    {
        private readonly IPublicHolidayService _holidayService;

        public PublicHolidayController(IPublicHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet("AllFuture")]
        public async Task<IActionResult> GetAllFuture()
        {
            var result = await _holidayService.GetAllFutureHolidaysAsync();
            return HandleResult(result, value => Ok(value));
        }
         [HttpGet("All")]
        public async Task<IActionResult> GetAllHolidys()
        {
            var result = await _holidayService.GetAllHolidaysAsync();
            return HandleResult(result, value => Ok(value));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _holidayService.GetByIdAsync(id);
            return HandleResult(result, value => Ok(value));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePublicHolidayRequest request)
        {
            var result = await _holidayService.CreateAsync(request);
            return HandleResult(result, value => CreatedAtAction(nameof(GetById), new { id = value.Id }, value));
        }

        [HttpPut("update/{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePublicHolidayRequest request)
        {
            var result = await _holidayService.UpdateAsync(id, request);
            return HandleResult(result, value => Ok(value));
        }
        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _holidayService.DeleteAsync(id);
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