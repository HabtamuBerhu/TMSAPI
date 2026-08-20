using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;

namespace TmsApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = "tms_auth";

    [HttpPost("login")]
    public IActionResult Login(
        [FromBody] LoginRequest request,
        [FromServices] IWebHostEnvironment env)
    {
        // Demo credentials
        if (request.Username != "admin" ||
            request.Password != "Password123!")
        {
            return Unauthorized(new
            {
                detail = "Invalid username or password."
            });
        }

        // Demo authentication token.
        // Later this can be replaced with a real JWT/session ID.
        var authToken = "header.payload.signature-demo-token";

        Response.Cookies.Append(
            AuthCookieName,
            authToken,
            new CookieOptions
            {
                HttpOnly = true,

                // HTTPS in production.
                // HTTP is allowed during local development.
                Secure = !env.IsDevelopment(),

                // Prevents cross-site cookie sending.
                SameSite = SameSiteMode.Strict,

                Expires = DateTimeOffset.UtcNow.AddHours(2),

                Path = "/"
            });

        return Ok(
            new UserProfileDto(
                "System Admin",
                "Admin"));
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // Browser automatically sends the HttpOnly cookie.
        if (Request.Cookies.TryGetValue(
                AuthCookieName,
                out var token) &&
            !string.IsNullOrWhiteSpace(token))
        {
            return Ok(
                new UserProfileDto(
                    "System Admin",
                    "Admin"));
        }

        return Unauthorized(new
        {
            detail = "Session expired or missing authentication cookie."
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
            AuthCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

        Response.Cookies.Delete(
            "XSRF-TOKEN",
            new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }
}