using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TmsApi.Services
{
    public interface IEnrollmentService
    {
        Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
        Task<EnrollmentRecord?> GetByIdAsync(string id);
        Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
        Task<bool> DeleteAsync(string id);
    }

    public class EnrollmentService : IEnrollmentService
    {
        // Thread-safe dictionary representing our in-memory data store
        private static readonly ConcurrentDictionary<string, EnrollmentRecord> _store = new();
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(ILogger<EnrollmentService> logger)
        {
            _logger = logger;
        }

        public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
        {
            // Session 2 - Guarding against duplicate enrollment attempts
            var existing = _store.Values
                .FirstOrDefault(e => e.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase) && 
                                     e.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                // Structured Logging: Queryable properties in brackets instead of string concatenation
                _logger.LogWarning(
                    "Duplicate enrollment attempt: Student {StudentId} is already enrolled in course {CourseCode}. Existing record ID: {EnrollmentId}",
                    studentId, courseCode, existing.Id
                );
                return Task.FromResult(existing);
            }

            var id = Guid.NewGuid().ToString("N")[..8];
            var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
            _store[id] = record;

            _logger.LogInformation(
                "Enrolled student {StudentId} in course {CourseCode}. Created record {EnrollmentId}",
                studentId, courseCode, id
            );

            return Task.FromResult(record);
        }

        public Task<EnrollmentRecord?> GetByIdAsync(string id)
        {
            if (_store.TryGetValue(id, out var record))
            {
                return Task.FromResult<EnrollmentRecord?>(record);
            }

            _logger.LogWarning("Enrollment record {EnrollmentId} not found", id);
            return Task.FromResult<EnrollmentRecord?>(null);
        }

        public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
        {
            IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
            return Task.FromResult(all);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var removed = _store.TryRemove(id, out _);

            if (removed)
            {
                _logger.LogInformation("Deleted enrollment record {EnrollmentId}", id);
            }
            else
            {
                _logger.LogWarning("Delete failed: enrollment record {EnrollmentId} not found", id);
            }

            return Task.FromResult(removed);
        }
    }

    // Immutable representation of an enrollment record (Session 2 / Tier 2)
    public record EnrollmentRecord(
        string Id,
        string StudentId,
        string CourseCode,
        DateTime EnrolledAt
    );
}