using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Common
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string Email { get; }
        string Role { get; }
        bool IsAuthenticated { get; }
    }
}
