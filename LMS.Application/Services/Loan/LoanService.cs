using LMS.Application.Common.Constants;
using LMS.Application.DTOs.Loan;
using LMS.Application.Interfaces.Common;
using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
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
        private readonly ILoanFinancialRepository _financialRepitory;

        public LoanService(
        ILoanRepository loanRepo,
        IWorkflowService workflow,
        IEligibilityService eligibility,
        IAuditService audit,ICurrentUserService currentUserService,
        ILoanFinancialRepository loanFinancialRepository)
        {
            _loanRepository = loanRepo;
            _workflowService = workflow;
            _eligibilityService = eligibility;
            _auditService = audit;
            _currentUserService = currentUserService;
            _financialRepitory = loanFinancialRepository;
        }

        public async Task<int> CreateDraftAsync(CreateLoanRequest request)
        {
            // 1. VALIDATION (basic)
            if (request.LoanAmount <= 0)
                throw new Exception("Invalid loan amount");

            if (request.TenureMonths <= 0)
                throw new Exception("Invalid tenure");

            if (request.MonthlyIncome <= 0)
                throw new Exception("Invalid income");

            // 2. GET CURRENT USER
            var userId = _currentUserService.UserId;

            // 3. CREATE LOAN
            var loan = new LoanApplication
            {
                UserId = userId,
                StatusId = LoanStatusConstants.Draft,
                PurposeId = request.PurposeId,
                EmploymentTypeId = request.EmploymentTypeId,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            var createdLoan = await _loanRepository.CreateAsync(loan);

            // 4. CREATE FINANCIAL DETAILS
            var financial = new LoanFinancialDetails
            {
                LoanApplicationId = createdLoan.Id,
                LoanAmount = request.LoanAmount,
                TenureMonths = request.TenureMonths,
                MonthlyIncome = request.MonthlyIncome,
                ExistingEMI = request.ExistingEMI
            };

            await _financialRepitory.AddAsync(financial);

            // 5. AUDIT
            await _auditService.LogAsync(
                "LoanApplication",
                createdLoan.Id,
                "CREATE_DRAFT",
                null,
                createdLoan
            );

            return createdLoan.Id;
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
