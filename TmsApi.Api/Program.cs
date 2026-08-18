using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Threading.Channels;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Middleware;
using TmsApi.Application.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Application.Transcripts;
using Microsoft.AspNetCore.Antiforgery;
using TmsApi.Infrastructure.Workers;
using  TmsApi.Application.Hubs;
using TmsApi.Application.Notifications;
var builder = WebApplication.CreateBuilder(args);


// ==========================
// SERVICES
// ==========================

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddAntiforgery(options =>
{
options.HeaderName = "X-XSRF-TOKEN";
});


builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();


builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();


builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly));

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);


// LoggingBehavior FIRST — it must wrap ValidationBehavior

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));


builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


// Enrollment Service
//builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();


builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(
        Console.WriteLine,
        LogLevel.Information)
    .EnableSensitiveDataLogging());


builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<GetEnrollmentService>();
//builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
//builder.Services.AddHostedService<TranscriptWorker>();

// ==========================
// API VERSIONING
// ==========================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader =
            new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
new BoundedChannelOptions(100)
{
FullMode = BoundedChannelFullMode.Wait
}));

// ==========================
// OPEN API DOCUMENTS
// ==========================

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});


builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
// Load allowed origins from appsettings.Development.json

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200"];

// Register the CORS policy in the Dependency Injection container
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()// Vital for HttpOnly auth cookies in Session 2
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});






// ==========================
// BUILD
// ==========================

var app = builder.Build();




app.Use(async (context, next) =>
{
if (context.User.Identity?.IsAuthenticated == true || context.
Request.Cookies.ContainsKey("tms_auth"))
{
var antiforgery = context.RequestServices
.GetRequiredService<IAntiforgery>();
var tokens = antiforgery.GetAndStoreTokens(context);
context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
new CookieOptions
{
HttpOnly = false, // MUST be false so Angular JavaScript can read it!
Secure = !builder.Environment.IsDevelopment(),
SameSite = SameSiteMode.Strict
});
}
await next(context);
});



app.UseCors("TmsClient");

app.MapHub<TmsHub>("/hubs/tms");

// ==========================
// DEVELOPMENT
// ==========================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(
                    ScalarTarget.CSharp,
                    ScalarClient.HttpClient);

        options.AddDocument("v1", "API Version 1.0");
        options.AddDocument("v2", "API Version 2.0");
    });
}



// ==========================
// MIDDLEWARE
// ==========================

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseMiddleware<V1DeprecationMiddleware>();


app.MapControllers();



// ==================================================
// MINIMAL API ENDPOINTS
// ==================================================


app.MapGet("/api/students",
async (TmsDbContext context, int page = 1) =>
{
    const int pageSize = 20;

    int adjustedPage = page < 1 ? 1 : page;


    var students = await context.Students
        .OrderBy(s => s.Name)
        .Skip((adjustedPage - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();


    return Results.Ok(students);
});



// ==========================
// ERROR TEST ROUTE
// ==========================


app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});


// ==========================
// DATABASE SEED
// ==========================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();

    context.Database.Migrate();


    List<Student> students = new();


    if (!context.Students.Any())
    {
        students = new List<Student>
        {
            new()
            {
                RegistrationNumber = "TMS-2026-0001",
                Name = "Alice Smith",
                GPA = 3.8m,
                IsActive = true
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0002",
                Name = "Bob Jones",
                GPA = 2.9m,
                IsActive = true
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0003",
                Name = "Charlie Brown",
                GPA = 3.4m,
                IsActive = false
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0004",
                Name = "Diana Prince",
                GPA = 3.9m,
                IsActive = true
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0005",
                Name = "Evan Wright",
                GPA = 2.5m,
                IsActive = true
            }
        };

        context.Students.AddRange(students);
        context.SaveChanges();
    }


    if (!context.Courses.Any())
    {
        var courses = new List<Course>
        {
            new()
            {
                Code = "CS-101",
                Title = "Introduction to Computer Science",
                Capacity = 30
            },

            new()
            {
                Code = "CS-201",
                Title = "Data Structures and Algorithms",
                Capacity = 25
            },

            new()
            {
                Code = "MAT-101",
                Title = "Calculus I",
                Capacity = 40
            }
        };


        context.Courses.AddRange(courses);
        context.SaveChanges();
    }
}

app.UseCors("AllowAngular");

app.Run();



// ==========================
// DTOs
// ==========================

public class CourseSummaryDto
{
    public string CourseTitle { get; set; } = string.Empty;

    public int EnrollmentCount { get; set; }
}