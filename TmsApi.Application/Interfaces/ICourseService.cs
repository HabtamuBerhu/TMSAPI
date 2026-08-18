using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseResponseDto>> GetAllAsync( CancellationToken ct);

    Task<CourseDetailDto?> GetByIdAsync(
        int id,
        CancellationToken ct);
//Task <EnrollmentResponseDto?> GetByStudentIdAsync(string StudentId,CancellationToken ct);
Task<CourseDetailDto?> GetByCodeAsync(string code, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct);

    Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct);

    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct);
}