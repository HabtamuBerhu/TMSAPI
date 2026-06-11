// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Scalar.AspNetCore;
// using TmsApi.Configuration;
// using TmsApi.Handlers;
// using TmsApi.Middleware;
// using TmsApi.Services;
// using TmsApi.Workers;

// var builder = WebApplication.CreateBuilder(args);


// // ==========================================
// // 1. SERVICES CONFIGURATION & REGISTRATION
// // ==========================================

// // Add controllers support to the API
// builder.Services.AddControllers();

// // Add support for OpenAPI documentation engine
// builder.Services.AddOpenApi();

// // Register the strongly-typed PaymentOptions with strict Startup validation (Session 2 / Exercise 3)
// builder.Services.AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();

// // Register application domain services with correct lifetimes
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// builder.Services.AddSingleton<EnrollmentWorker>();

// // Register standard exception mapping (ProblemDetails RFC 9457)
// builder.Services.AddProblemDetails();

// // Register temporary authentication scheme & handler (Session 1 / Exercise 1)
// builder.Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

// builder.Services.AddAuthorization();

// // Turn on development host checks for lifetime mismatches (Session 2 / Exercise 2)
// builder.Host.UseDefaultServiceProvider(options =>
// {
//     options.ValidateScopes = true;
//     options.ValidateOnBuild = true;
// });

// var app = builder.Build();
// app.UseExceptionHandler();

// // ==========================================
// // 2. MIDDLEWARE RECEPTIVITY & ORDERING
// // ==========================================

// // RULE: Custom Request Logging Middleware must be at the very front of the pipeline (Session 1 / Exercise 1B)
// app.UseMiddleware<RequestLoggingMiddleware>();

// // Configure Error Handling & Diagnostics toggling per environment (Session 3 / Exercise 7)
// if (app.Environment.IsDevelopment())
// {
//     // Enable OpenAPI documents and the interactive Scalar API Reference panel
//     app.MapOpenApi();
//     app.MapScalarApiReference();
    
//     // Developer diagnostics fallback
//     app.UseDeveloperExceptionPage();
// }
// else
// {
//     // Production Mode: Intercept exceptions, mapping them clean using ProblemDetails RFC specs
//     app.UseExceptionHandler();
// }

// // Convert empty response codes (e.g., standard 404s, 401s) into unified JSON (ProblemDetails)
// app.UseStatusCodePages();

// app.UseHttpsRedirection();

// // Setup routing infrastructure
// app.UseRouting();

// // Authentication and Authorization must be configured after UseRouting but before Endpoint Maps
// app.UseAuthentication();
// app.UseAuthorization();

// // ==========================================
// // 3. ROUTE REGISTRATIONS
// // ==========================================

// // Map Controller routes
// app.MapControllers();

// // Endpoint A: Secure Assessment results (Session 1 / Exercise 1)
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// })).RequireAuthorization();

// // Endpoint B: Enrollment Worker Background processing smoke tester (Session 2 / Exercise 2)
// app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
// {
//     worker.ProcessBatch();
//     return Results.Ok("Batch processed successfully without memory leaks or captive dependency locks.");
// });

// // Endpoint C: Trigger error test mapping (Session 3 / Exercise 6)
// app.MapGet("/api/error", () =>
// {
//     throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
// });

// app.Run();
using Scalar.AspNetCore;
using TmsApi;
var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddControllers();
//builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// SERVICES
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();





// Dev-only tools
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Global error handling (always ON)


// MIDDLEWARE (ORDER IS VERY IMPORTANT)
app.UseExceptionHandler();     // MUST be first
app.UseStatusCodePages();     // optional but recommended

app.MapControllers();

// TEST ROUTE (must be AFTER MapControllers)
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});








app.Run();