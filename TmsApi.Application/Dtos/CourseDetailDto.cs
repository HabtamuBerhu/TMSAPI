namespace TmsApi.Application.Dtos;

public class CourseDetailDto
{
    public string Id { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int MaxCapacity { get; set; }

    public int EnrollmentCount { get; set; }

    public List<LinkDto> Links { get; set; } = new();
}