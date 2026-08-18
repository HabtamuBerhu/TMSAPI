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
}