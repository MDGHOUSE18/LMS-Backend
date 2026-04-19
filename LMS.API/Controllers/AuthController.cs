using Azure.Core;
using LMS.Application.DTOs.Auth;
using LMS.Application.DTOs.common;
using LMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
        {
            var result = await _authService.RegisterAsync(requestDto);

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                ErrorCode = null,
                Message = result,
                Data = null,
                Timestamp = DateTime.UtcNow
            });
        }
        [HttpGet("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            return Ok(new { message = result });
        }

        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto request)
        {
            await _authService.ResendVerificationEmailAsync(request.Email);
            return Ok(new { message = "Verification email sent." });
        }
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();
            var message = await _authService.LoginAsync(requestDto, ipAddress, userAgent);

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                ErrorCode = null,
                Message = message,
                Data = null,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto requestDto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();
            var authResponse = await _authService.VerifyOtpAsync(requestDto, ipAddress, userAgent);

            return Ok(new ApiResponse<AuthResponseDto>
            {
                StatusCode = 200,
                ErrorCode = null,
                Message = "OTP verified successfully",
                Data = authResponse,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var authResponse = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

            return Ok(new ApiResponse<AuthResponseDto>
            {
                StatusCode = 200,
                ErrorCode = null,
                Message = "Token refreshed successfully",
                Data = authResponse,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdClaim.Value);
            if (userId==0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    StatusCode = 401,
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Invalid or missing user claim in token",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }
            await _authService.LogoutAsync(userId);
            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                ErrorCode = null,
                Message = "User logged out successfully",
                Data = null,
                Timestamp = DateTime.UtcNow
            });
        }
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            // ✅ Service returns raw token string
            var token = await _authService.ForgotPasswordAsync(Email);

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                ErrorCode = null,
                Message = "Password reset link sent",
                Data = new { token }, // ✅ Wrap raw token in object
                Timestamp = DateTime.UtcNow
            });
        }
        [HttpPost("reset-passport")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordRequest requestDto)
        {
            var message = await _authService.ResetPasswordAsync(requestDto);

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                ErrorCode = null,
                Message = message,
                Data = null,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
