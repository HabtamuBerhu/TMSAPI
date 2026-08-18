namespace TmsApi.Application.Dtos;

public class CourseResponseDto
{
    public string Id { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int MaxCapacity { get; set; }
}