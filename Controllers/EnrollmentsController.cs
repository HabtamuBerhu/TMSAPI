using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        // Constructor Injection for Scoped Service
        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        // GET /api/enrollments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await _enrollmentService.GetAllAsync();
            return Ok(enrollments);
        }

        // GET /api/enrollments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var record = await _enrollmentService.GetByIdAsync(id);
            return record is not null ? Ok(record) : NotFound();
        }

        // POST /api/enrollments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.StudentId) || string.IsNullOrWhiteSpace(request.CourseCode))
            {
                return BadRequest(new { error = "StudentId and CourseCode are required." });
            }

            var record = await _enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
            
            // Returns 201 Created status, containing the Location header pointing back to the resource
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }

        // DELETE /api/enrollments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _enrollmentService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }

    public record CreateEnrollmentRequest(string StudentId, string CourseCode);
}