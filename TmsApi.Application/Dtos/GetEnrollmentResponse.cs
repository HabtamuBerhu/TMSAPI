namespace TmsApi.Application.Dtos;

public class GetEnrollmentResponseDto
{
    public int Id { get; set; }

    public int StudentId { get; set; } 

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }
}