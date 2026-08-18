using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
//using TmsApi.Infrastructure.Persistence;
namespace TmsApi.Application.Services;


public interface IEnrollmentService
{
    Task<IReadOnlyList<EnrollmentRecord>> GetByStudentIdAsync(
    string studentId,
    CancellationToken ct);
    Task<EnrollmentRecord> EnrollAsync(
        string studentId,
        string courseCode);

    Task<EnrollmentRecord?> GetByIdAsync(
        string id);

    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();

    Task<IReadOnlyList<EnrollmentRecord>> GetByCourseAsync(
        string courseCode);

    Task<bool> DeleteAsync(
        string id);

Task<bool> ExistsAsync(
    string studentId,
    string courseCode,
    CancellationToken ct);
    
}
public class EnrollmentService : IEnrollmentService
{
    private static readonly ConcurrentDictionary<string, EnrollmentRecord> _store = new();

    private readonly ILogger<EnrollmentService> _logger;


    public EnrollmentService(
        ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }





public Task<IReadOnlyList<EnrollmentRecord>> GetByStudentIdAsync(
    string studentId,
    CancellationToken ct)
{
    IReadOnlyList<EnrollmentRecord> result = _store.Values
        .Where(e => e.StudentId.Equals(
            studentId,
            StringComparison.OrdinalIgnoreCase))
        .ToList();

    return Task.FromResult(result);
}


public Task<bool> ExistsAsync(
    string studentId,
    string courseCode,
    CancellationToken ct)
    
{
    var exists = _store.Values.Any(e =>
        e.StudentId.Equals(
            studentId,
            StringComparison.OrdinalIgnoreCase)
        &&
        e.CourseCode.Equals(
            courseCode,
            StringComparison.OrdinalIgnoreCase));

    return Task.FromResult(exists);
}


    public Task<EnrollmentRecord> EnrollAsync(
        string studentId,
        string courseCode)
    {
        var existing = _store.Values
            .FirstOrDefault(e =>
                e.StudentId.Equals(
                    studentId,
                    StringComparison.OrdinalIgnoreCase)
                &&
                e.CourseCode.Equals(
                    courseCode,
                    StringComparison.OrdinalIgnoreCase));


        if (existing is not null)
        {
            _logger.LogWarning(
                "Student {StudentId} already enrolled in {CourseCode}",
                studentId,
                courseCode);

            return Task.FromResult(existing);
        }


        var id = Guid.NewGuid()
            .ToString("N")[..8];


        var record = new EnrollmentRecord(
            id,
            studentId,
            courseCode,
            DateTime.UtcNow);


        _store[id] = record;


        return Task.FromResult(record);
    }



    public Task<EnrollmentRecord?> GetByIdAsync(
        string id)
    {
        _store.TryGetValue(id, out var record);

        return Task.FromResult(record);
    }



    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> result =
            _store.Values.ToList();

        return Task.FromResult(result);
    }



    // NEW METHOD FOR STEP 6
    public Task<IReadOnlyList<EnrollmentRecord>> GetByCourseAsync(
        string courseCode)
    {
        IReadOnlyList<EnrollmentRecord> result =
            _store.Values
                .Where(e =>
                    e.CourseCode.Equals(
                        courseCode,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();


        return Task.FromResult(result);
    }



    public Task<bool> DeleteAsync(
        string id)
    {
        var removed =
            _store.TryRemove(id, out _);


        return Task.FromResult(removed);
    }
}



public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt
);