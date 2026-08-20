namespace TmsApi.Application.Dtos;

public class GetEnrollmentResponseDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public string StudentName { get; set; }


    public string CourseName { get; set; }

    public string Status { get; set; }
    public DateTime EnrolledAt { get; set; }
}