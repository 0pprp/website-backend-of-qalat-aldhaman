using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;
using QalatAldhaman.Store.Api.Entities;
using QalatAldhaman.Store.Api.Services;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AdminAuthController(
        AppDbContext context,
        IPasswordHasher<AdminUser> passwordHasher,
        JwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    [HttpPost("setup")]
    public async Task<ActionResult<AdminUserDto>> Setup(AdminSetupRequestDto request)
    {
        if (await _context.AdminUsers.AnyAsync())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "تم الإعداد مسبقاً" });
        }

        var expectedSetupKey = _configuration["AdminSetup:SetupKey"];
        if (string.IsNullOrWhiteSpace(expectedSetupKey) || request.SetupKey != expectedSetupKey)
        {
            return Unauthorized(new { message = "SetupKey غير صحيح" });
        }

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "الرجاء إدخال جميع الحقول المطلوبة" });
        }

        var user = new AdminUser
        {
            Username = request.Username,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new AdminUserDto { Username = user.Username, FullName = user.FullName });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AdminLoginResponseDto>> Login(AdminLoginRequestDto request)
    {
        var user = await _context.AdminUsers.SingleOrDefaultAsync(u => u.Username == request.Username);

        if (user is not null)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result != PasswordVerificationResult.Failed)
            {
                var token = _jwtTokenService.GenerateToken(user);
                return Ok(new AdminLoginResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    FullName = user.FullName,
                });
            }
        }

        return Unauthorized(new { message = "بيانات الدخول غير صحيحة" });
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AdminUserDto> Me()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var fullName = User.FindFirst("full_name")?.Value ?? string.Empty;

        return Ok(new AdminUserDto { Username = username, FullName = fullName });
    }
}
