using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeeting.DTOs;
using SmartMeeting.Services.Interfaces;
using System.Security.Claims;

namespace SmartMeeting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterAsync(registerDto, ipAddress);

            if (result.Success)
            {
                SetRefreshTokenCookie(result.RefreshToken!);
                return Ok(new
                {
                    result.Success,
                    result.Message,
                    result.AccessToken,
                    result.ExpiresAt,
                    result.User
                });
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.LoginAsync(loginDto, ipAddress);

            if (result.Success)
            {
                SetRefreshTokenCookie(result.RefreshToken!);
                return Ok(new
                {
                    result.Success,
                    result.Message,
                    result.AccessToken,
                    result.ExpiresAt,
                    result.User
                });
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { success = false, message = "Refresh token is required" });
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

            if (result.Success)
            {
                SetRefreshTokenCookie(result.RefreshToken!);
                return Ok(new
                {
                    result.Success,
                    result.Message,
                    result.AccessToken,
                    result.ExpiresAt
                });
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto tokenDto)
        {
            var token = tokenDto.RefreshToken ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { success = false, message = "Token is required" });
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RevokeTokenAsync(token, ipAddress);

            if (result)
            {
                return Ok(new { success = true, message = "Token revoked successfully" });
            }

            return BadRequest(new { success = false, message = "Failed to revoke token" });
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "User not found" });
            }

            var result = await _authService.LogoutAsync(userId);
            if (result)
            {
                Response.Cookies.Delete("refreshToken");
                return Ok(new { success = true, message = "Logged out successfully" });
            }

            return BadRequest(new { success = false, message = "Failed to logout" });
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "User not found" });
            }

            var user = await _authService.GetUserInfoAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, data = user });
        }

        /// <summary>
        /// Change password
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "User not found" });
            }

            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (result)
            {
                return Ok(new { success = true, message = "Password changed successfully" });
            }

            return BadRequest(new { success = false, message = "Failed to change password" });
        }

        private string GetIpAddress()
        {
            return Request.Headers.ContainsKey("X-Forwarded-For")
                ? Request.Headers["X-Forwarded-For"].ToString()
                : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
