namespace HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;

public class UpdatePublicHolidayRequest
{
    public DateOnly Date { get; set; }
    public required string Name { get; set; }
}
