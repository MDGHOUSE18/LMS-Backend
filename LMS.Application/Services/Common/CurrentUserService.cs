using LMS.Application.Interfaces.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Common
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int UserId =>
            int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? id
                : 0;

        public string Email =>
            User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        public string Role =>
            User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated ?? false;
    }
}
