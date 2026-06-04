using LMS.Application.Common.Constants;
using LMS.Application.DTOs.Loan;
using LMS.Application.Interfaces.Common;
using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Guid> CreateDraftAsync(CreateLoanRequest request)
        {
            // 1. VALIDATION (basic)
            if (request.LoanAmount <= 0)
                throw new Exception("Invalid loan amount");

            if (request.TenureMonths <= 0)
                throw new Exception("Invalid tenure");

            if (request.MonthlyIncome <= 0)
                throw new Exception("Invalid income");

            // 2. GET CURRENT USER
            var userId = _currentUserService.GetCurrentUserId();

            // 3. CREATE LOAN using factory method
            var loan = LoanApplication.Create(
                userId: userId,
                createdBy: userId,
                loanAmount: request.LoanAmount,
                tenureMonths: request.TenureMonths,
                interestRate: 12.0m,
                monthlyIncome: request.MonthlyIncome,
                existingEMI: request.ExistingEMI,
                purpose: request.PurposeId.ToString(),
                employmentType: request.EmploymentTypeId.ToString()
            );

            var createdLoan = await _loanRepository.AddAsync(loan);

            // 4. AUDIT
            await _auditService.LogAsync(
                "LoanApplication",
                createdLoan.Id,
                "CREATE_DRAFT",
                null,
                createdLoan
            );

            return createdLoan.Id;
        }

        public async Task SubmitLoanAsync(Guid loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found", nameof(loanId));

            var userId = _currentUserService.GetCurrentUserId();
            
            // Submit the loan
            loan.Submit(userId);

            _loanRepository.Update(loan);
            
            await _auditService.LogAsync(
                "LoanApplication",
                loan.Id,
                "SUBMIT",
                null,
                loan
            );
        }

        public async Task UpdateDraftAsync(UpdateLoanRequest request)
        {
            var loan = await _loanRepository.GetByIdAsync(request.LoanId);
            if (loan == null)
                throw new ArgumentException("Loan not found", nameof(request.LoanId));

            if (loan.Status != LoanStatusEnum.Draft)
                throw new InvalidOperationException("Only draft loans can be updated");

            // Update financial details through domain method or direct property access
            // Note: In rich domain model, we should use a domain method
            loan.UpdateFinancialDetails(
                request.LoanAmount,
                request.TenureMonths,
                loan.InterestRate,
                request.MonthlyIncome,
                request.ExistingEMI
            );

            _loanRepository.Update(loan);
            
            await _auditService.LogAsync(
                "LoanApplication",
                loan.Id,
                "UPDATE_DRAFT",
                null,
                loan
            );
        }

        public async Task<LoanApplication?> GetByIdAsync(Guid loanId)
        {
            return await _loanRepository.GetByIdAsync(loanId);
        }

        public async Task<List<LoanApplication>> GetLoansByUserIdAsync(Guid userId)
        {
            return await _loanRepository.GetByUserIdAsync(userId);
        }

        public async Task<List<LoanApplication>> GetPendingApplicationsAsync()
        {
            return await _loanRepository.GetPendingApplicationsAsync();
        }

        public async Task<List<LoanApplication>> GetAllLoansAsync()
        {
            return await _loanRepository.GetAllLoansAsync();
        }

        public async Task ApproveLoanAsync(Guid loanId, Guid approvedBy)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found", nameof(loanId));

            if (loan.Status == LoanStatusEnum.Submitted)
            {
                loan.MoveToUnderReview(approvedBy);
            }

            loan.Approve(approvedBy, "Approved from service");
            _loanRepository.Update(loan);
        }

        public async Task RejectLoanAsync(Guid loanId, string reason, Guid rejectedBy)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found", nameof(loanId));

            loan.Reject(rejectedBy, reason);
            _loanRepository.Update(loan);
        }

        public async Task GenerateEmiScheduleAsync(Guid loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found", nameof(loanId));

            loan.CalculateAndStoreEMI();
            _loanRepository.Update(loan);
        }
    }
}
