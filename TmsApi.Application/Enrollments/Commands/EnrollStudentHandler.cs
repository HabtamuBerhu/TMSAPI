using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Services;
using TmsApi.Application.Interfaces;
namespace TmsApi.Application.Enrollments.Commands;

public class EnrollStudentHandler(
    IEnrollmentService enrollmentService,
    ICourseService courseService)
    : IRequestHandler<EnrollStudentCommand, Result<EnrollmentCreated, EnrollmentError>>
{
    public async Task<Result<EnrollmentCreated, EnrollmentError>> Handle(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        var course = await courseService.GetByCodeAsync(
            command.CourseCode,
            ct);

        if (course is null)
        {
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseNotFound(command.CourseCode));
        }

        // Count enrollments using the enrollment service
        var enrollments = await enrollmentService.GetByCourseAsync(
            command.CourseCode);

        if (enrollments.Count >= course.MaxCapacity)
        {
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseFull(
                    course.Title,
                    course.MaxCapacity));
        }

        if (await enrollmentService.ExistsAsync(
                command.StudentId.ToString(),
                command.CourseCode,
                ct))
        {
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.AlreadyEnrolled(
                    command.StudentId,
                    command.CourseCode));
        }

        var enrollment = await enrollmentService.EnrollAsync(
            command.StudentId.ToString(),
            command.CourseCode);

        return Result<EnrollmentCreated, EnrollmentError>.Success(
            new EnrollmentCreated(
               int.Parse(enrollment.Id),
                int.Parse(enrollment.StudentId),
                enrollment.CourseCode));
    }
}