using LMS.Application.Interfaces.Services.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class AuditService : IAuditService
    {
        public Task LogAsync(string entity, int entityId, string action, object? before, object? after)
        {
            throw new NotImplementedException();
        }
    }
}
