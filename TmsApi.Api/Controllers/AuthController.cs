
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TmsDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TmsDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }

    // ============================================================
    // REGISTER
    // ============================================================

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role);

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            // Prevent account enumeration
            return Ok(new
            {
                message = "Registration request received."
            });
        }

        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            var errors =
                result.Errors.Select(e => e.Description);

            return BadRequest(new { errors });
        }

        // Ensure requested role exists
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(request.Role));
        }

        await _userManager.AddToRoleAsync(
            user,
            request.Role);

        return Ok(new
        {
            message = "Registration successful."
        });
    }

    // ============================================================
    // LOGIN
    // ============================================================

    public record LoginRequest(
        string Email,
        string Password);

    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        // Check account lockout
        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail =
                    "Account locked due to multiple failed login attempts. Try again in 15 minutes."
            });
        }

        // Check password
        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        // Reset failed attempt counter
        await _userManager.ResetAccessFailedCountAsync(user);

        // Get user's roles
        var roles =
            await _userManager.GetRolesAsync(user);

        // Generate JWT access token
        var accessToken =
            _tokenService.GenerateJwt(user, roles);

        // ========================================================
        // CREATE INITIAL REFRESH TOKEN
        // ========================================================

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        // Return tokens + user information
        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            accessToken,
            refreshToken = refreshToken.Token
        });
    }

    // ============================================================
    // REFRESH TOKEN
    // ============================================================

    public record RefreshRequest(
        string RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        // Find refresh token in database
        var storedToken =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // ========================================================
        // TOKEN THEFT DETECTION
        // ========================================================

        // If an already-used refresh token is submitted,
        // revoke ALL refresh tokens belonging to that user.
        if (storedToken.IsUsed)
        {
            var userTokens =
                await _context.RefreshTokens
                    .Where(rt =>
                        rt.UserId == storedToken.UserId)
                    .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail =
                    "Token theft detected. All user sessions revoked."
            });
        }

        // ========================================================
        // CHECK EXPIRATION / REVOCATION
        // ========================================================

        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail =
                    "Refresh token expired or revoked."
            });
        }

        // ========================================================
        // MARK OLD TOKEN AS USED
        // ========================================================

        storedToken.IsUsed = true;

        // ========================================================
        // CREATE NEW REFRESH TOKEN
        // ========================================================

        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        // ========================================================
        // GET USER
        // ========================================================

        var user =
            await _userManager.FindByIdAsync(
                storedToken.UserId);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "User not found."
            });
        }

        // Get user's roles
        var roles =
            await _userManager.GetRolesAsync(user);

        // Generate new access token
        var newAccessToken =
            _tokenService.GenerateJwt(user, roles);

        // Save old token as used and new token
        await _context.SaveChangesAsync();

        // Return new token pair
        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }
}



