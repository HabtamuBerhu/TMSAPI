using System;
namespace TmsApi.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public decimal? Grade { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Exercise 9
    public bool IsArchived { get; set; }
    public string Status { get; set; } = "Pending";

    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;


}