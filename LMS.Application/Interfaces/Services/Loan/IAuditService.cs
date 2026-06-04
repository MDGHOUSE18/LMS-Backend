using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Services.Loan
{
    public interface IAuditService
    {
        Task LogAsync(string entity, Guid entityId, string action, object? before, object? after);
    }
}
