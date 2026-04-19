using LMS.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Security
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user,String roleName);

        string GenerateRefreshToken();

        string HashToken(string token);
    }
}
