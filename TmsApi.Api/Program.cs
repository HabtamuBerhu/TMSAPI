using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Threading.Channels;

using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Notifications;
using TmsApi.Application.Services;
using TmsApi.Application.Transcripts;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Middleware;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// SERVICES
// ============================================================

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddSignalR();


// ============================================================
// ANTIFORGERY
// ============================================================

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});


// ============================================================
// TRANSCRIPT SERVICES
// ============================================================

builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();

builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();


// ============================================================
// MEDIATR
// ============================================================

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly));


// ============================================================
// FLUENT VALIDATION
// ============================================================

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);


// ============================================================
// MEDIATR PIPELINE BEHAVIORS
// ============================================================

// LoggingBehavior FIRST.
// It wraps ValidationBehavior.

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));


// ============================================================
// EXCEPTION HANDLING
// ============================================================

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"));

    options.LogTo(
        Console.WriteLine,
        LogLevel.Information);

    options.EnableSensitiveDataLogging();
});


// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<GetEnrollmentService>();


// ============================================================
// API VERSIONING
// ============================================================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(1, 0);

        options.AssumeDefaultVersionWhenUnspecified =
            true;

        options.ReportApiVersions = true;

        options.ApiVersionReader =
            new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl = true;
    });


// ============================================================
// TRANSCRIPT CHANNEL
// ============================================================

builder.Services.AddSingleton(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));


// ============================================================
// OPENAPI - VERSION 1
// ============================================================

builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.ShouldInclude = description =>
            description.GroupName == "v1";
    });


// ============================================================
// OPENAPI - VERSION 2
// ============================================================

builder.Services.AddOpenApi(
    "v2",
    options =>
    {
        options.ShouldInclude = description =>
            description.GroupName == "v2";
    });


// ============================================================
// CORS
// ============================================================

var allowedOrigins =
    builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>()
        ??
        [
            "http://localhost:4200"
        ];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(
                TimeSpan.FromMinutes(10));
    });
});


// ============================================================
// BUILD
// ============================================================

var app = builder.Build();


// ============================================================
// EXCEPTION HANDLER
// ============================================================

app.UseExceptionHandler();


// ============================================================
// STATUS CODE PAGES
// ============================================================

app.UseStatusCodePages();


// ============================================================
// CORS
// ============================================================

// IMPORTANT:
// This must be before MapControllers/MapHub.

app.UseCors("TmsClient");


// ============================================================
// XSRF / ANTIFORGERY COOKIE
// ============================================================
//
// Angular needs to read XSRF-TOKEN.
// Therefore HttpOnly MUST be false for this cookie.
//
// The authentication cookie remains HttpOnly.

app.Use(async (context, next) =>
{
    if (context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery =
            context.RequestServices
                .GetRequiredService<IAntiforgery>();

        var tokens =
            antiforgery.GetAndStoreTokens(context);

        if (!string.IsNullOrEmpty(tokens.RequestToken))
        {
            context.Response.Cookies.Append(
                "XSRF-TOKEN",
                tokens.RequestToken,
                new CookieOptions
                {
                    HttpOnly = false,

                    Secure =
                        !builder.Environment.IsDevelopment(),

                    SameSite =
                        SameSiteMode.Strict,

                    Path = "/"
                });
        }
    }

    await next();
});


// ============================================================
// SIGNALR
// ============================================================

app.MapHub<TmsHub>("/hubs/tms");

app.MapHub<TmsHub>("/hubs/tms").RequireCors("TmsClient");
// ============================================================
// DEVELOPMENT / OPENAPI
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);

        options.AddDocument(
            "v1",
            "API Version 1.0");

        options.AddDocument(
            "v2",
            "API Version 2.0");
    });
}


// ============================================================
// CUSTOM MIDDLEWARE
// ============================================================

app.UseMiddleware<V1DeprecationMiddleware>();


// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// STUDENTS MINIMAL API
// ============================================================

app.MapGet(
    "/api/students",
    async (
        TmsDbContext context,
        int page = 1) =>
    {
        const int pageSize = 20;

        int adjustedPage =
            page < 1
                ? 1
                : page;

        var students =
            await context.Students
                .OrderBy(s => s.Name)
                .Skip(
                    (adjustedPage - 1)
                    * pageSize)
                .Take(pageSize)
                .ToListAsync();

        return Results.Ok(students);
    });


// ============================================================
// ERROR TEST ROUTE
// ============================================================

app.MapGet(
    "/api/error",
    () =>
    {
        throw new TmsDatabaseException(
            "Simulated database failure for ProblemDetails testing");
    });


// ============================================================
// DATABASE MIGRATION + SEED
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<TmsDbContext>();

    context.Database.Migrate();


    // --------------------------------------------------------
    // STUDENTS
    // --------------------------------------------------------

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new()
            {
                RegistrationNumber =
                    "TMS-2026-0001",

                Name =
                    "Alice Smith",

                GPA =
                    3.8m,

                IsActive =
                    true
            },

            new()
            {
                RegistrationNumber =
                    "TMS-2026-0002",

                Name =
                    "Bob Jones",

                GPA =
                    2.9m,

                IsActive =
                    true
            },

            new()
            {
                RegistrationNumber =
                    "TMS-2026-0003",

                Name =
                    "Charlie Brown",

                GPA =
                    3.4m,

                IsActive =
                    false
            },

            new()
            {
                RegistrationNumber =
                    "TMS-2026-0004",

                Name =
                    "Diana Prince",

                GPA =
                    3.9m,

                IsActive =
                    true
            },

            new()
            {
                RegistrationNumber =
                    "TMS-2026-0005",

                Name =
                    "Evan Wright",

                GPA =
                    2.5m,

                IsActive =
                    true
            }
        };

        context.Students.AddRange(students);

        context.SaveChanges();
    }


    // --------------------------------------------------------
    // COURSES
    // --------------------------------------------------------

    if (!context.Courses.Any())
    {
        var courses = new List<Course>
        {
            new()
            {
                Code =
                    "CS-101",

                Title =
                    "Introduction to Computer Science",

                Capacity =
                    30
            },

            new()
            {
                Code =
                    "CS-201",

                Title =
                    "Data Structures and Algorithms",

                Capacity =
                    25
            },

            new()
            {
                Code =
                    "MAT-101",

                Title =
                    "Calculus I",

                Capacity =
                    40
            }
        };

        context.Courses.AddRange(courses);

        context.SaveChanges();
    }
}


// ============================================================
// RUN
// ============================================================

app.Run();


// ============================================================
// DTOs
// ============================================================

public class CourseSummaryDto
{
    public string CourseTitle { get; set; }
        = string.Empty;

    public int EnrollmentCount { get; set; }
}