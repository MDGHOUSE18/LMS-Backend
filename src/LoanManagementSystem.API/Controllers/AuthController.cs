using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LoanManagementSystem.Core.DTOs;
using LoanManagementSystem.Core.Entities;
using LoanManagementSystem.Core.Interfaces;
using BCrypt.Net;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                // Validate email format
                if (!IsValidEmail(dto.Email))
                    return BadRequest(new { message = "Invalid email format" });

                // Check for existing user
                var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                    return Conflict(new { message = "Email already registered" });

                var existingPhone = await _userRepository.GetByPhoneAsync(dto.Phone);
                if (existingPhone != null)
                    return Conflict(new { message = "Phone number already registered" });

                // Validate password
                if (dto.Password.Length < 8 || !HasSpecialChar(dto.Password))
                    return BadRequest(new { message = "Password must be at least 8 characters with special characters" });

                // Create user
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = dto.Email.ToLower(),
                    Phone = dto.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 10),
                    Name = dto.Name,
                    DateOfBirth = dto.DateOfBirth,
                    Role = dto.Role,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);

                // Log audit
                await CreateAuditLog(user.UserId, "Create", "User", user.UserId, "User registered");

                var response = new UserDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Phone = user.Phone,
                    Name = user.Name,
                    DateOfBirth = user.DateOfBirth,
                    Role = user.Role,
                    IsVerified = user.IsVerified
                };

                return CreatedAtAction(nameof(Register), new { userId = user.UserId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Invalid credentials" });

                // Generate JWT tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // Log audit
                await CreateAuditLog(user.UserId, "Login", "User", user.UserId, $"User logged in from IP: {GetClientIp()}");

                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        Phone = user.Phone,
                        Name = user.Name,
                        DateOfBirth = user.DateOfBirth,
                        Role = user.Role,
                        IsVerified = user.IsVerified
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Login failed", error = ex.Message });
            }
        }

        private string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment123456!"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("role", user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "LoanManagementSystem",
                audience: _configuration["Jwt:Audience"] ?? "LoanManagementSystem",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool HasSpecialChar(string password)
        {
            return password.Any(c => "@#$%^&+=!".Contains(c));
        }

        private string GetClientIp()
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
                   Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private async Task CreateAuditLog(Guid? userId, string action, string entityType, Guid entityId, string details)
        {
            var auditLog = new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IpAddress = GetClientIp()
            };
            await _auditLogRepository.AddAsync(auditLog);
        }
    }
}
