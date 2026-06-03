using LMS.Application.Interfaces.Services.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class WorkflowService : IWorkflowService
    {
        public Task ChangeStatusAsync(int loanId, int toStatusId, string? comments = null)
        {
            throw new NotImplementedException();
        }
    }
}
