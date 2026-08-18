namespace TmsApi.Application.Dtos;

public class EnrollmentResponseDto
{
    public string Id { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;

    public string CourseCode { get; set; } = string.Empty;

    public DateTime EnrolledAt { get; set; }
}