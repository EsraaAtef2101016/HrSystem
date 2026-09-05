using System.ComponentModel.DataAnnotations;
//C:\Users\DELL\source\repos\HrSystem\HrSystem.Application\Features\EmployeeParticipation\DTO\RequestDto
using HrSystem.Application.Features.EmployeeParticipation.DTO.RequestDto;
namespace HrSystem.Application.Features.EmployeeParticipation.DTO.RequestDto;

    public class UpdateGlobalPolicyRequest
    {
        public bool IsSelfOptOutAllowed { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Cooldown days cannot be a negative number.")]
        public int CooldownDays { get; set; }
    }
