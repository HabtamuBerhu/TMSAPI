using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class TestController(TmsDbContext context) : ControllerBase
{
    // ==============================
    // 1. Deferred execution demo
    // ==============================
    // GET /api/reports/deferred-students
    [HttpGet("deferred-students")]
    public IActionResult TestDeferred()
    {
        var query = context.Students
            .Where(s => s.GPA >= 3.0m)
            .OrderBy(s => s.Name);

        var results = query.ToList();

        return Ok(results);
    }

    // ==============================
    // 2. Top courses report
    // ==============================
    // GET /api/reports/top-courses
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses()
    {
        var report = await context.Enrollments
            .GroupBy(e => new { e.CourseId, e.Course.Title })
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new CourseSummaryDto
            {
                CourseTitle = g.Key.Title,
                EnrollmentCount = g.Count()
            })
            .ToListAsync();

        if (!report.Any())
        {
            return NotFound(new ProblemDetails
            {
                Title = "No report data",
                Detail = "No enrollment data found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(report);
    }

    // ==============================
    // 3. Paginated students
    // ==============================
    // GET /api/reports/students?page=1
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(int page = 1)
    {
        const int pageSize = 20;
        int adjustedPage = page < 1 ? 1 : page;

        var students = await context.Students
            .OrderBy(s => s.Name)
            .Skip((adjustedPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(students);
    }

    // ==============================
    // 4. Translation failure demo
    // ==============================
    // GET /api/reports/honor-roll-test
    [HttpGet("honor-roll-test")]
    public IActionResult TestTranslationFail()
    {
        try
        {
            var students = context.Students
                .Where(s => s.GPA >= 3.5m) // FIXED: made translatable
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Query failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    // ==============================
    // helper
    // ==============================
    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }
}