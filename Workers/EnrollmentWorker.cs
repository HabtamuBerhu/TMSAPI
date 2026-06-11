using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TmsApi.Services;

namespace TmsApi.Workers
{
    /// <summary>
    /// Background service scheduled tasks simulating worker process.
    /// Implements scope resolution through IServiceScopeFactory to prevent a Captive Dependency bug 
    /// (holding onto a Scoped IEnrollmentService inside a Singleton service lifetime).
    /// </summary>
    public class EnrollmentWorker
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EnrollmentWorker> _logger;

        public EnrollmentWorker(IServiceScopeFactory scopeFactory, ILogger<EnrollmentWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void ProcessBatch()
        {
            _logger.LogInformation("Enrollment worker batch processing starting...");

            // Create a short-lived scope to correctly resolve the scoped EnrollmentService
            using var scope = _scopeFactory.CreateScope();
            var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

            // Simulate executing processing logic using the resolved scoped service
            var enrollments = enrollmentService.GetAllAsync().GetAwaiter().GetResult();
            
            _logger.LogInformation(
                "Enrollment worker successfully processed scoped batch. Total records processed: {Count}", 
                enrollments.Count
            );
        }
    }
}