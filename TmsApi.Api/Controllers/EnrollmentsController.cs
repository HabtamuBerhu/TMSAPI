namespace TmsApi.Api.Controllers;

using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Asp.Versioning;
using MediatR;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs; 

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("1.0")]
public class EnrollmentsController(
    GetEnrollmentService enrollmentService,
    IMediator mediator,
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            command,
            ct);


        return result.Match<IActionResult>(

            onSuccess: created =>
                CreatedAtAction(
                    nameof(GetSchedule),
                    new
                    {
                        studentId = created.StudentId
                    },
                    created),


            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" =>
                        StatusCodes.Status404NotFound,

                    "course_full" or "already_enrolled" =>
                        StatusCodes.Status409Conflict,

                    _ =>
                        StatusCodes.Status400BadRequest
                };


                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }


    [HttpGet("{studentId}/schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId),
            ct);


        return Ok(schedule);
    }


    [HttpGet]
    public async Task<IActionResult> GetEnrollments(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result =
            await enrollmentService.GetEnrollmentsAsync(
                request,
                ct);

        return Ok(result);
    }


    // NEW: Approve enrollment
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(
        string id,
        CancellationToken ct)
    {
        // Your existing approval logic should go here.
        // The database commit must succeed before broadcasting.

        // Example:
        // await mediator.Send(new ApproveEnrollmentCommand(id), ct);

        await hubContext.Clients.All
            .ReceiveEnrollmentStatusUpdated(
                id,
                "Approved");

        return NoContent();
    }
}