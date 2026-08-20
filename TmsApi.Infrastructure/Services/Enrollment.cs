using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Application.Services;

public class GetEnrollmentService(TmsDbContext context)
{

    public Task<PagedResponse<GetEnrollmentResponseDto>> GetEnrollmentsAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        var enrollments = context.Enrollments
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new GetEnrollmentResponseDto
            {
                Id = c.Id,
                StudentId = c.StudentId,
                CourseId = c.CourseId,
                StudentName = c.Student.Name,
                CourseName = c.Course.Title,
                Status = c.Status,
                EnrolledAt = c.EnrolledAt
            })
            .ToList();


        var response = new PagedResponse<GetEnrollmentResponseDto>
        {
            Items = enrollments,
            TotalCount = enrollments.Count(),
            Page = request.Page,
            PageSize = request.PageSize
        };


        return Task.FromResult(response);
    }

    // POST: Create a new enrollment
    public async Task<Enrollment> CreateEnrollmentAsync(
        int studentId,
        int courseId,
        CancellationToken ct)
    {
        // Check whether the student exists
        // var studentExists = await context.Students
        //     .AnyAsync(s => s.Id == studentId, ct);

        // if (!studentExists)
        // {
        //     throw new KeyNotFoundException(
        //         $"Student with ID {studentId} was not found.");
        // }

        // Check whether the course exists
        // var courseExists = await context.Courses
        //     .AnyAsync(c => c.Id == courseId, ct);

        // if (!courseExists)
        // {
        //     throw new KeyNotFoundException(
        //         $"Course with ID {courseId} was not found.");
        // }

        // // Check for duplicate enrollment
        // var alreadyEnrolled = await context.Enrollments
        //     .AnyAsync(
        //         e => e.StudentId == studentId &&
        //              e.CourseId == courseId,
        //         ct);
        // if (alreadyEnrolled)
        // {
        //     throw new InvalidOperationException(
        //         "The student is already enrolled in this course.");
        // }

        // Create enrollment
        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            Status = "Pending",
            EnrolledAt = DateTime.UtcNow
        };

        // Add to database
        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        return enrollment;
    }


}