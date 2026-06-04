using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class WorkflowService : IWorkflowService
    {
        public async Task ChangeStatusAsync(int loanId, int toStatusId, string? comments = null)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Submit loan application - triggers eligibility check
        /// </summary>
        public async Task<bool> SubmitLoanAsync(LoanApplication loan)
        {
            loan.Submit(Guid.Empty);
            return true;
        }

        /// <summary>
        /// Move to Under Review after eligibility passes
        /// </summary>
        public async Task<bool> MoveToReviewAsync(LoanApplication loan)
        {
            loan.MoveToUnderReview(Guid.Empty);
            return true;
        }

        /// <summary>
        /// Approve loan - generates EMI schedule
        /// </summary>
        public async Task<bool> ApproveLoanAsync(LoanApplication loan, Guid approvedByUserId)
        {
            if (loan.Status == LoanStatusEnum.Submitted)
            {
                loan.MoveToUnderReview(approvedByUserId);
            }
            loan.Approve(approvedByUserId);
            return true;
        }

        /// <summary>
        /// Reject loan with reason
        /// </summary>
        public async Task<bool> RejectLoanAsync(LoanApplication loan, Guid rejectedByUserId, string reason)
        {
            loan.Reject(rejectedByUserId, reason);
            return true;
        }

        /// <summary>
        /// Mark loan as disbursed - starts EMI tracking
        /// </summary>
        public async Task<bool> DisburseLoanAsync(LoanApplication loan)
        {
            loan.Disburse(Guid.Empty);
            return true;
        }

        /// <summary>
        /// Validate status transition rules
        /// </summary>
        public bool IsValidTransition(string fromStatus, string toStatus)
        {
            if (!Enum.TryParse<LoanStatusEnum>(fromStatus, true, out var from) ||
                !Enum.TryParse<LoanStatusEnum>(toStatus, true, out var to))
            {
                return false;
            }

            return (from, to) switch
            {
                (LoanStatusEnum.Draft, LoanStatusEnum.Submitted) => true,
                (LoanStatusEnum.Submitted, LoanStatusEnum.UnderReview) => true,
                (LoanStatusEnum.UnderReview, LoanStatusEnum.Approved) => true,
                (LoanStatusEnum.UnderReview, LoanStatusEnum.Rejected) => true,
                (LoanStatusEnum.Submitted, LoanStatusEnum.Rejected) => true,
                (LoanStatusEnum.Approved, LoanStatusEnum.Disbursed) => true,
                _ => false
            };
        }

        /// <summary>
        /// Get all valid next statuses for current status
        /// </summary>
        public List<string> GetNextValidStatuses(string currentStatus)
        {
            if (!Enum.TryParse<LoanStatusEnum>(currentStatus, true, out var status))
                return new List<string>();

            return status switch
            {
                LoanStatusEnum.Draft => new List<string> { LoanStatusEnum.Submitted.ToString() },
                LoanStatusEnum.Submitted => new List<string> { LoanStatusEnum.UnderReview.ToString(), LoanStatusEnum.Rejected.ToString() },
                LoanStatusEnum.UnderReview => new List<string> { LoanStatusEnum.Approved.ToString(), LoanStatusEnum.Rejected.ToString() },
                LoanStatusEnum.Approved => new List<string> { LoanStatusEnum.Disbursed.ToString() },
                _ => new List<string>()
            };
        }
    }
}
