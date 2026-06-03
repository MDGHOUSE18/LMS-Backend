using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class WorkflowService : IWorkflowService
    {
        // BRD Status Constants
        public const string DRAFT = "Draft";
        public const string SUBMITTED = "Submitted";
        public const string UNDER_REVIEW = "Under Review";
        public const string APPROVED = "Approved";
        public const string REJECTED = "Rejected";
        public const string DISBURSED = "Disbursed";

        private static readonly List<string> ValidStatuses = new()
        {
            DRAFT, SUBMITTED, UNDER_REVIEW, APPROVED, REJECTED, DISBURSED
        };

        public async Task ChangeStatusAsync(int loanId, int toStatusId, string? comments = null)
        {
            throw new NotImplementedException("This method signature needs to be updated to use Guid");
        }

        /// <summary>
        /// Submit loan application - triggers eligibility check
        /// </summary>
        public async Task<bool> SubmitLoanAsync(LoanApplication loan)
        {
            if (loan.Status != DRAFT)
                throw new InvalidOperationException($"Cannot submit loan with status '{loan.Status}'. Only Draft loans can be submitted.");

            // Transition to Submitted (eligibility will be checked by service layer)
            loan.Status = SUBMITTED;
            loan.AppliedDate = DateTime.UtcNow;
            loan.LastModifiedAt = DateTime.UtcNow;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Move to Under Review after eligibility passes
        /// </summary>
        public async Task<bool> MoveToReviewAsync(LoanApplication loan)
        {
            if (loan.Status != SUBMITTED)
                throw new InvalidOperationException($"Cannot move to review. Current status: '{loan.Status}'");

            loan.Status = UNDER_REVIEW;
            loan.LastModifiedAt = DateTime.UtcNow;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Approve loan - generates EMI schedule
        /// </summary>
        public async Task<bool> ApproveLoanAsync(LoanApplication loan, Guid approvedByUserId)
        {
            if (loan.Status != UNDER_REVIEW)
                throw new InvalidOperationException($"Cannot approve loan. Current status: '{loan.Status}'");

            loan.Status = APPROVED;
            loan.ApprovedDate = DateTime.UtcNow;
            loan.ApprovedBy = approvedByUserId;
            loan.LastModifiedAt = DateTime.UtcNow;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Reject loan with reason
        /// </summary>
        public async Task<bool> RejectLoanAsync(LoanApplication loan, Guid rejectedByUserId, string reason)
        {
            if (loan.Status != SUBMITTED && loan.Status != UNDER_REVIEW)
                throw new InvalidOperationException($"Cannot reject loan. Current status: '{loan.Status}'");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required");

            loan.Status = REJECTED;
            loan.RejectedDate = DateTime.UtcNow;
            loan.RejectedBy = rejectedByUserId;
            loan.RejectionReason = reason;
            loan.LastModifiedAt = DateTime.UtcNow;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Mark loan as disbursed - starts EMI tracking
        /// </summary>
        public async Task<bool> DisburseLoanAsync(LoanApplication loan)
        {
            if (loan.Status != APPROVED)
                throw new InvalidOperationException($"Cannot disburse loan. Current status: '{loan.Status}'");

            loan.Status = DISBURSED;
            loan.DisbursedDate = DateTime.UtcNow;
            loan.LastModifiedAt = DateTime.UtcNow;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate status transition rules
        /// </summary>
        public bool IsValidTransition(string fromStatus, string toStatus)
        {
            var allowedTransitions = new Dictionary<string, List<string>>
            {
                { DRAFT, new List<string> { SUBMITTED } },
                { SUBMITTED, new List<string> { UNDER_REVIEW, REJECTED } },
                { UNDER_REVIEW, new List<string> { APPROVED, REJECTED } },
                { APPROVED, new List<string> { DISBURSED } },
                { REJECTED, new List<string>() }, // Terminal state
                { DISBURSED, new List<string>() }  // Terminal state
            };

            if (!allowedTransitions.ContainsKey(fromStatus))
                return false;

            return allowedTransitions[fromStatus].Contains(toStatus);
        }

        /// <summary>
        /// Get all valid next statuses for current status
        /// </summary>
        public List<string> GetNextValidStatuses(string currentStatus)
        {
            var allowedTransitions = new Dictionary<string, List<string>>
            {
                { DRAFT, new List<string> { SUBMITTED } },
                { SUBMITTED, new List<string> { UNDER_REVIEW, REJECTED } },
                { UNDER_REVIEW, new List<string> { APPROVED, REJECTED } },
                { APPROVED, new List<string> { DISBURSED } },
                { REJECTED, new List<string>() },
                { DISBURSED, new List<string>() }
            };

            return allowedTransitions.TryGetValue(currentStatus, out var statuses) 
                ? statuses 
                : new List<string>();
        }
    }
}
