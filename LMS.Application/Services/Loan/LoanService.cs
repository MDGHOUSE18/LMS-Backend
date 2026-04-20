using LMS.Application.DTOs.Loan;
using LMS.Application.Interfaces.Common;
using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Application.Interfaces.Services.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IWorkflowService _workflowService;
        private readonly IEligibilityService _eligibilityService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public LoanService(
        ILoanRepository loanRepo,
        IWorkflowService workflow,
        IEligibilityService eligibility,
        IAuditService audit,ICurrentUserService currentUserService)
        {
            _loanRepository = loanRepo;
            _workflowService = workflow;
            _eligibilityService = eligibility;
            _auditService = audit;
            _currentUserService = currentUserService;
        }

        public Task<int> CreateDraftAsync(CreateLoanRequest request)
        {
            throw new NotImplementedException();
        }

        public Task SubmitLoanAsync(int loanId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDraftAsync(UpdateLoanRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
