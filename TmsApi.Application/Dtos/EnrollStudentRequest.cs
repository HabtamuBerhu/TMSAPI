namespace TmsApi.Application.Dtos;

public class EnrollStudentRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
}