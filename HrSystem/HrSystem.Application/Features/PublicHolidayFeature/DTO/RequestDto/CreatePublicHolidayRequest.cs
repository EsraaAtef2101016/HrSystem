namespace HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;
public class CreatePublicHolidayRequest
{
    public DateOnly Date { get; set; }
    public  string Name { get; set; }
}
