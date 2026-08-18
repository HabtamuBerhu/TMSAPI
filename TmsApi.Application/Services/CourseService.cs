using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Services;

public class CourseService: ICourseService
{
    private readonly List<Course> _courses = new();


    public Task<IEnumerable<CourseResponseDto>> GetAllAsync(
        CancellationToken ct)
    {
        var result = _courses.Select(c => new CourseResponseDto
        {
            Id = c.Id.ToString(),
            Code = c.Code,
            Title = c.Title,
            MaxCapacity = c.Capacity
        });

       return Task.FromResult(result);
    }


    public Task<CourseDetailDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        var course = _courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return Task.FromResult<CourseDetailDto?>(null);


        var result = new CourseDetailDto
        {
            Id = course.Id.ToString(),
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.Capacity,

            EnrollmentCount = 0,

            Links = new List<LinkDto>()
        };


        return Task.FromResult<CourseDetailDto?>(result);
    }

        public Task<CourseDetailDto?> GetByCodeAsync(
        string CourseCode,
        CancellationToken ct)
    {
        var course = _courses.FirstOrDefault(c => c.Code == CourseCode);

        if (course == null)
            return Task.FromResult<CourseDetailDto?>(null);


        var result = new CourseDetailDto
        {
            Id = course.Id.ToString(),
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.Capacity,

            EnrollmentCount = 0,

            Links = new List<LinkDto>()
        };


        return Task.FromResult<CourseDetailDto?>(result);
    }


    public Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            Capacity = request.MaxCapacity
        };


        _courses.Add(course);


        var result = new CourseResponseDto
        {
            Id = course.Id.ToString(),
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.Capacity
        };


        return Task.FromResult(result);
    }


    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct)
    {
        var exists = _courses.Any(c =>
            c.Code.Equals(
                code,
                StringComparison.OrdinalIgnoreCase));


        return Task.FromResult(exists);
    }


    public Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        var courses = _courses
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id.ToString(),
                Code = c.Code,
                Title = c.Title,
                MaxCapacity = c.Capacity
            })
            .ToList();


        var response = new PagedResponse<CourseResponseDto>
        {
            Items = courses,
            TotalCount = courses.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };


        return Task.FromResult(response);
    }
}