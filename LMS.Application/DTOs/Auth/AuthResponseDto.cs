using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; }= default!;
        public DateTime AccessTokenExpiry { get; set; }
        public UserDto User { get; set; }=default!;
    }
}
