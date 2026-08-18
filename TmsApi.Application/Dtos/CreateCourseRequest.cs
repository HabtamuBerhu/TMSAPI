namespace TmsApi.Application.Dtos;

public class CreateCourseRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int MaxCapacity { get; set; }
}