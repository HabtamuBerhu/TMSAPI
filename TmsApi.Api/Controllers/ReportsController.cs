using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;


[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
    // 1. How many active students have GPA >= 3.0?
    [HttpGet("active-high-gpa-count")]
    public async Task<IActionResult> GetActiveHighGpaCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new
        {
            ActiveStudentsWithGpaAbove3 = count
        });
    }

    // 2. Which courses have the most enrollments?
    [HttpGet("course-enrollments")]
    public async Task<IActionResult> GetCourseEnrollmentCounts()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        if (!list.Any())
        {
            return NotFound(new ProblemDetails
            {
                Title = "No courses found",
                Detail = "There are no course enrollment records available.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(list);
    }

    // 3. Average GPA per course
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> GetAverageGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        if (!list.Any())
        {
            return NotFound(new ProblemDetails
            {
                Title = "No enrollment data",
                Detail = "No GPA information is available.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(list);
    }

    // 4A. Students without enrollments (NOT EXISTS)
    [HttpGet("students-without-enrollments")]
    public async Task<IActionResult> GetStudentsWithoutEnrollments()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // 4B. Students without enrollments (LEFT JOIN)
    [HttpGet("students-without-enrollments-leftjoin")]
    public async Task<IActionResult> GetStudentsWithoutEnrollmentsLeftJoin()
    {
        var list = await context.Students
            .GroupJoin(
                context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (student, enrollments) => new
                {
                    student,
                    enrollments
                })
            .Where(x => !x.enrollments.Any())
            .Select(x => x.student.Name)
            .ToListAsync();

        return Ok(list);
    }
}