using MediatR;
using TmsApi.Application.Services;

namespace TmsApi.Application.Enrollments.Queries;

public sealed class GetStudentScheduleHandler(
    IEnrollmentService repo)
    : IRequestHandler<GetStudentScheduleQuery, ScheduleDto>
{
    public async Task<ScheduleDto> Handle(
        GetStudentScheduleQuery query,
        CancellationToken ct)
    {
        var enrollments = await repo.GetByStudentIdAsync(
            query.StudentId.ToString(),
            ct);

        var items = enrollments
            .Select(e => new ScheduleItemDto(
                e.CourseCode,      // Course Code
                e.CourseCode,      // Temporary title
                "TBD"))            // Schedule not implemented yet
            .ToList();

        return new ScheduleDto(
            query.StudentId.ToString(),
            items);
    }
}