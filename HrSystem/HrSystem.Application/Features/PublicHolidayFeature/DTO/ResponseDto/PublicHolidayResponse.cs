namespace HrSystem.Application.DTO.Features.PublicHolidayFeature.ResponseDto;

public class PublicHolidayResponse
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public required string Name { get; set; }
}
