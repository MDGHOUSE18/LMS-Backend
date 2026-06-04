using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Lookup;
using LMS.Domain.Entities.Workflow;
using LMS.Domain.Enums;
using LMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LMS.Domain.Entities.Loan
{
    /// <summary>
    /// Rich domain model for Loan Application following Domain-Driven Design principles.
    /// Encapsulates business logic for EMI calculation, eligibility validation, and state transitions.
    /// </summary>
    public class LoanApplication
    {
        // Private backing fields for encapsulation
        private readonly List<Document> _documents = new();
        private readonly List<Payment> _payments = new();
        private readonly List<LoanStatusHistory> _statusHistories = new();
        private readonly List<AmortizationSchedule> _amortizationSchedules = new();

        #region Properties

        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }
        
        /// <summary>
        /// Current status of the loan application
        /// </summary>
        public LoanStatusEnum Status { get; private set; } = LoanStatusEnum.Draft;
        
        public string? Purpose { get; private set; } // Home, Auto, Personal, Education
        public string EmploymentType { get; private set; } = default!; // Salaried, Self-employed
        
        public DateTime AppliedDate { get; private set; }
        public DateTime? ApprovedDate { get; private set; }
        public DateTime? RejectedDate { get; private set; }
        public DateTime? DisbursedDate { get; private set; }
        public string? RejectionReason { get; private set; }
        
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }
        
        public Guid CreatedBy { get; private set; }
        public Guid? LastModifiedBy { get; private set; }
        public Guid? ApprovedBy { get; private set; }
        public Guid? RejectedBy { get; private set; }

        // Financial details (1:1 relationship)
        public decimal LoanAmount { get; private set; }
        public int TenureMonths { get; private set; }
        public decimal InterestRate { get; private set; }
        public decimal MonthlyIncome { get; private set; }
        public decimal? ExistingEMI { get; private set; }
        
        /// <summary>
        /// Calculated EMI amount based on the formula: EMI = [P × R × (1+R)ⁿ] / [(1+R)ⁿ – 1]
        /// where P = Principal, R = monthly interest rate, n = tenure in months
        /// </summary>
        public decimal? CalculatedEMI { get; private set; }
        
        /// <summary>
        /// Total interest payable over the loan tenure
        /// </summary>
        public decimal? TotalInterestPayable { get; private set; }
        
        /// <summary>
        /// Total amount payable (Principal + Interest)
        /// </summary>
        public decimal? TotalPayableAmount { get; private set; }

        // Navigation properties
        public virtual User? User { get; private set; }
        public virtual LoanFinancialDetails? FinancialDetails { get; private set; }
        public virtual ICollection<Document> Documents => _documents.AsReadOnly();
        public virtual ICollection<Payment> Payments => _payments.AsReadOnly();
        public virtual ICollection<LoanStatusHistory> StatusHistories => _statusHistories.AsReadOnly();
        public virtual ICollection<AmortizationSchedule> AmortizationSchedules => _amortizationSchedules.AsReadOnly();

        #endregion

        // Private parameterless constructor for EF Core
        private LoanApplication() { }

        #region Factory Method

        /// <summary>
        /// Creates a new loan application in Draft state
        /// </summary>
        public static LoanApplication Create(
            Guid userId,
            Guid createdBy,
            decimal loanAmount,
            int tenureMonths,
            decimal interestRate,
            decimal monthlyIncome,
            decimal? existingEMI,
            string? purpose = null,
            string employmentType = "Salaried")
        {
            var loan = new LoanApplication
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedBy = createdBy,
                LoanAmount = loanAmount,
                TenureMonths = tenureMonths,
                InterestRate = interestRate,
                MonthlyIncome = monthlyIncome,
                ExistingEMI = existingEMI,
                Purpose = purpose,
                EmploymentType = employmentType,
                CreatedAt = DateTime.UtcNow,
                Status = LoanStatusEnum.Draft
            };

            // Calculate and store EMI and amortization schedule
            loan.CalculateAndStoreEMI();

            return loan;
        }

        #endregion

        #region EMI Calculation

        /// <summary>
        /// Calculates EMI using the formula: EMI = [P × R × (1+R)ⁿ] / [(1+R)ⁿ – 1]
        /// P = Principal loan amount
        /// R = Monthly interest rate (annual rate / 12 / 100)
        /// n = Loan tenure in months
        /// </summary>
        public void CalculateAndStoreEMI()
        {
            if (LoanAmount <= 0 || TenureMonths <= 0 || InterestRate < 0)
                throw new DomainException("Invalid loan parameters for EMI calculation.");

            // Convert annual interest rate to monthly rate (as decimal)
            decimal monthlyInterestRate = InterestRate / 12 / 100;
            int numberOfInstallments = TenureMonths;

            decimal emi;
            
            if (monthlyInterestRate == 0)
            {
                // Edge case: 0% interest loan
                emi = LoanAmount / numberOfInstallments;
            }
            else
            {
                // EMI Formula: [P × R × (1+R)ⁿ] / [(1+R)ⁿ – 1]
                double r = (double)monthlyInterestRate;
                double n = numberOfInstallments;
                
                double powerTerm = Math.Pow(1 + r, n);
                double numerator = (double)LoanAmount * r * powerTerm;
                double denominator = powerTerm - 1;
                
                emi = (decimal)(numerator / denominator);
            }

            CalculatedEMI = Math.Round(emi, 2);
            TotalPayableAmount = Math.Round(CalculatedEMI.Value * numberOfInstallments, 2);
            TotalInterestPayable = Math.Round(TotalPayableAmount.Value - LoanAmount, 2);

            // Generate amortization schedule
            GenerateAmortizationSchedule();
        }

        /// <summary>
        /// Generates the complete amortization schedule with principal and interest breakdown
        /// </summary>
        private void GenerateAmortizationSchedule()
        {
            _amortizationSchedules.Clear();
            
            if (!CalculatedEMI.HasValue)
                throw new DomainException("EMI must be calculated before generating amortization schedule.");

            decimal monthlyInterestRate = InterestRate / 12 / 100;
            decimal outstandingBalance = LoanAmount;
            DateTime startDate = DateTime.Today;

            for (int month = 1; month <= TenureMonths; month++)
            {
                // Calculate interest component for this month
                decimal interestComponent = Math.Round(outstandingBalance * monthlyInterestRate, 2);
                
                // Calculate principal component
                decimal principalComponent = Math.Round(CalculatedEMI.Value - interestComponent, 2);
                
                // Adjust for final month rounding differences
                if (month == TenureMonths)
                {
                    principalComponent = outstandingBalance;
                    CalculatedEMI = Math.Round(principalComponent + interestComponent, 2);
                }

                // Update outstanding balance
                outstandingBalance = Math.Round(outstandingBalance - principalComponent, 2);
                if (outstandingBalance < 0) outstandingBalance = 0;

                // Calculate due date (same day each month)
                DateTime dueDate = startDate.AddMonths(month);
                // Adjust for month-end dates
                if (startDate.Day > 28 && dueDate.Day != startDate.Day)
                {
                    dueDate = dueDate.AddDays(-dueDate.Day).AddDays(1).AddDays(-1);
                }

                var schedule = new AmortizationSchedule(
                    Id,
                    month,
                    dueDate,
                    CalculatedEMI.Value,
                    principalComponent,
                    interestComponent,
                    outstandingBalance
                );

                _amortizationSchedules.Add(schedule);
            }
        }

        #endregion

        #region Eligibility Validation

        /// <summary>
        /// Checks if the applicant is eligible for the loan based on BRD criteria:
        /// - Age: 21-60 years
        /// - Minimum Salary: ₹25,000
        /// - Debt-to-Income Ratio: ≤50%
        /// - Loan-to-Income Ratio: ≤20x annual income
        /// </summary>
        /// <param name="dateOfBirth">Applicant's date of birth</param>
        /// <returns>EligibilityResult with detailed validation outcomes</returns>
        public EligibilityResult CheckEligibility(DateTime? dateOfBirth)
        {
            var result = new EligibilityResult
            {
                LoanApplicationId = (int)Id.GetHashCode(), // Temporary mapping
                EvaluatedAt = DateTime.UtcNow
            };

            var failureReasons = new List<string>();

            // Age validation (21-60 years)
            if (dateOfBirth.HasValue)
            {
                int age = CalculateAge(dateOfBirth.Value);
                result.Age = age;
                
                if (age < 21)
                    failureReasons.Add($"Applicant age ({age}) is below minimum requirement of 21 years.");
                else if (age > 60)
                    failureReasons.Add($"Applicant age ({age}) exceeds maximum limit of 60 years.");
            }
            else
            {
                failureReasons.Add("Date of birth is required for eligibility check.");
            }

            // Minimum salary validation (₹25,000)
            result.MonthlyIncome = MonthlyIncome;
            if (MonthlyIncome < 25000)
                failureReasons.Add($"Monthly income (₹{MonthlyIncome}) is below minimum requirement of ₹25,000.");

            // Debt-to-Income Ratio validation (≤50%)
            decimal totalMonthlyEMI = CalculatedEMI ?? 0;
            if (ExistingEMI.HasValue)
                totalMonthlyEMI += ExistingEMI.Value;

            decimal dtiRatio = MonthlyIncome > 0 ? (totalMonthlyEMI / MonthlyIncome) * 100 : 100;
            result.DTI = Math.Round(dtiRatio, 2);

            if (dtiRatio > 50)
                failureReasons.Add($"Debt-to-Income ratio ({dtiRatio:F2}%) exceeds maximum limit of 50%.");

            // Loan-to-Income Ratio validation (≤20x annual income)
            decimal annualIncome = MonthlyIncome * 12;
            decimal loanToIncomeRatio = annualIncome > 0 ? LoanAmount / annualIncome : decimal.MaxValue;
            result.LoanToIncomeRatio = Math.Round(loanToIncomeRatio, 2);

            if (loanToIncomeRatio > 20)
                failureReasons.Add($"Loan-to-Income ratio ({loanToIncomeRatio:F2}x) exceeds maximum limit of 20x annual income.");

            result.IsEligible = !failureReasons.Any();
            result.Remarks = result.IsEligible 
                ? "Applicant meets all eligibility criteria." 
                : string.Join(" ", failureReasons);

            if (!result.IsEligible)
                throw new IneligibleException(failureReasons);

            return result;
        }

        /// <summary>
        /// Calculates age from date of birth
        /// </summary>
        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;
            
            // Adjust if birthday hasn't occurred yet this year
            if (dateOfBirth.Date > today.AddYears(-age))
                age--;
            
            return age;
        }

        #endregion

        #region State Transitions (Workflow)

        /// <summary>
        /// Submits the loan application for review.
        /// Transition: Draft → Submitted
        /// </summary>
        public void Submit(Guid submittedBy)
        {
            ValidateStateTransition(LoanStatusEnum.Submitted);

            // Business rules for submission
            if (_amortizationSchedules.Count == 0)
                throw new DomainException("Cannot submit loan without EMI calculation.");

            if (string.IsNullOrEmpty(Purpose))
                throw new DomainException("Loan purpose is required for submission.");

            Status = LoanStatusEnum.Submitted;
            AppliedDate = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = submittedBy;

            AddStatusHistory(LoanStatusEnum.Draft, LoanStatusEnum.Submitted, submittedBy, "Application submitted for review");
        }

        /// <summary>
        /// Approves the loan application after underwriting review.
        /// Transition: Under Review → Approved
        /// </summary>
        public void Approve(Guid approvedBy, string? comments = null)
        {
            ValidateStateTransition(LoanStatusEnum.Approved);

            Status = LoanStatusEnum.Approved;
            ApprovedDate = DateTime.UtcNow;
            ApprovedBy = approvedBy;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = approvedBy;
            RejectionReason = null;

            AddStatusHistory(LoanStatusEnum.UnderReview, LoanStatusEnum.Approved, approvedBy, comments ?? "Loan approved");
        }

        /// <summary>
        /// Rejects the loan application.
        /// Transition: Submitted/Under Review → Rejected
        /// </summary>
        public void Reject(Guid rejectedBy, string reason)
        {
            ValidateStateTransition(LoanStatusEnum.Rejected);

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("Rejection reason is required.");

            Status = LoanStatusEnum.Rejected;
            RejectedDate = DateTime.UtcNow;
            RejectedBy = rejectedBy;
            RejectionReason = reason;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = rejectedBy;

            AddStatusHistory(Status == LoanStatusEnum.Submitted ? LoanStatusEnum.Submitted : LoanStatusEnum.UnderReview, 
                           LoanStatusEnum.Rejected, rejectedBy, $"Rejected: {reason}");
        }

        /// <summary>
        /// Marks the loan as disbursed after approval and documentation.
        /// Transition: Approved → Disbursed
        /// </summary>
        public void Disburse(Guid disbursedBy)
        {
            ValidateStateTransition(LoanStatusEnum.Disbursed);

            Status = LoanStatusEnum.Disbursed;
            DisbursedDate = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = disbursedBy;

            AddStatusHistory(LoanStatusEnum.Approved, LoanStatusEnum.Disbursed, disbursedBy, "Loan amount disbursed");
        }

        /// <summary>
        /// Moves the application to under review status.
        /// Transition: Submitted → Under Review
        /// </summary>
        public void MoveToUnderReview(Guid reviewedBy, string? comments = null)
        {
            ValidateStateTransition(LoanStatusEnum.UnderReview);

            Status = LoanStatusEnum.UnderReview;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = reviewedBy;

            AddStatusHistory(LoanStatusEnum.Submitted, LoanStatusEnum.UnderReview, reviewedBy, comments ?? "Application moved to under review");
        }

        /// <summary>
        /// Validates if a state transition is allowed based on the workflow rules
        /// </summary>
        private void ValidateStateTransition(LoanStatusEnum targetStatus)
        {
            bool isValidTransition = (Status, targetStatus) switch
            {
                (LoanStatusEnum.Draft, LoanStatusEnum.Submitted) => true,
                (LoanStatusEnum.Submitted, LoanStatusEnum.UnderReview) => true,
                (LoanStatusEnum.UnderReview, LoanStatusEnum.Approved) => true,
                (LoanStatusEnum.UnderReview, LoanStatusEnum.Rejected) => true,
                (LoanStatusEnum.Submitted, LoanStatusEnum.Rejected) => true,
                (LoanStatusEnum.Approved, LoanStatusEnum.Disbursed) => true,
                _ => false
            };

            if (!isValidTransition)
                throw new InvalidStateException(
                    $"Invalid state transition from '{Status}' to '{targetStatus}'. " +
                    $"Allowed transitions: Draft→Submitted, Submitted→UnderReview, " +
                    $"UnderReview→Approved/Rejected, Submitted→Rejected, Approved→Disbursed");
        }

        /// <summary>
        /// Adds a status history entry for audit trail
        /// </summary>
        private void AddStatusHistory(LoanStatusEnum fromStatus, LoanStatusEnum toStatus, Guid changedBy, string? comments)
        {
            // Note: In real implementation, you'd map LoanStatusEnum to LoanStatus entity IDs
            var history = new LoanStatusHistory
            {
                LoanApplicationId = (int)Id.GetHashCode(), // Temporary mapping
                ToStatusId = (int)toStatus,
                ChangedBy = (int)changedBy.GetHashCode(), // Temporary mapping
                ChangedAt = DateTime.UtcNow,
                Comments = comments
            };

            _statusHistories.Add(history);
        }

        #endregion

        #region Additional Domain Methods

        /// <summary>
        /// Updates loan financial details (only allowed in Draft state)
        /// </summary>
        public void UpdateFinancialDetails(
            decimal loanAmount,
            int tenureMonths,
            decimal interestRate,
            decimal monthlyIncome,
            decimal? existingEMI)
        {
            if (Status != LoanStatusEnum.Draft)
                throw new InvalidStateException("Financial details can only be modified in Draft state.");

            LoanAmount = loanAmount;
            TenureMonths = tenureMonths;
            InterestRate = interestRate;
            MonthlyIncome = monthlyIncome;
            ExistingEMI = existingEMI;

            // Recalculate EMI and regenerate amortization schedule
            CalculateAndStoreEMI();
            
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a document to the loan application
        /// </summary>
        public void AddDocument(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _documents.Add(document);
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the current outstanding principal balance
        /// </summary>
        public decimal GetOutstandingBalance()
        {
            if (_amortizationSchedules.Count == 0)
                return LoanAmount;

            var lastPaidSchedule = _amortizationSchedules
                .Where(s => s.Status == "Paid")
                .OrderByDescending(s => s.InstallmentNumber)
                .FirstOrDefault();

            if (lastPaidSchedule == null)
                return LoanAmount;

            return lastPaidSchedule.OutstandingBalance;
        }

        /// <summary>
        /// Gets the next due installment
        /// </summary>
        public AmortizationSchedule? GetNextDueInstallment()
        {
            return _amortizationSchedules
                .Where(s => s.Status == "Pending" && s.DueDate >= DateTime.Today)
                .OrderBy(s => s.InstallmentNumber)
                .FirstOrDefault();
        }

        #endregion
    }
}
