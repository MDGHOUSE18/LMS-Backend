using LMS.Domain.Exceptions;

namespace LMS.Domain.Entities.Loan
{
    /// <summary>
    /// Represents a single installment in the loan amortization schedule.
    /// </summary>
    public class AmortizationSchedule
    {
        public Guid Id { get; private set; }
        public Guid LoanApplicationId { get; private set; }
        
        /// <summary>
        /// Installment number (1-based)
        /// </summary>
        public int InstallmentNumber { get; private set; }
        
        /// <summary>
        /// Due date for this installment
        /// </summary>
        public DateTime DueDate { get; private set; }
        
        /// <summary>
        /// Total EMI amount for this installment
        /// </summary>
        public decimal EMIAmount { get; private set; }
        
        /// <summary>
        /// Principal component of the EMI
        /// </summary>
        public decimal PrincipalComponent { get; private set; }
        
        /// <summary>
        /// Interest component of the EMI
        /// </summary>
        public decimal InterestComponent { get; private set; }
        
        /// <summary>
        /// Outstanding principal balance after this installment
        /// </summary>
        public decimal OutstandingBalance { get; private set; }
        
        /// <summary>
        /// Payment status: Pending, Paid, PartiallyPaid, Overdue
        /// </summary>
        public string Status { get; private set; } = "Pending";
        
        /// <summary>
        /// Actual payment date if paid
        /// </summary>
        public DateTime? PaidDate { get; private set; }
        
        /// <summary>
        /// Amount actually paid
        /// </summary>
        public decimal? PaidAmount { get; private set; }
        
        public DateTime CreatedAt { get; private set; }

        // Navigation property
        public virtual LoanApplication? LoanApplication { get; private set; }

        // Private parameterless constructor for EF Core
        private AmortizationSchedule() { }

        public AmortizationSchedule(
            Guid loanApplicationId,
            int installmentNumber,
            DateTime dueDate,
            decimal emiAmount,
            decimal principalComponent,
            decimal interestComponent,
            decimal outstandingBalance)
        {
            Id = Guid.NewGuid();
            LoanApplicationId = loanApplicationId;
            InstallmentNumber = installmentNumber;
            DueDate = dueDate;
            EMIAmount = emiAmount;
            PrincipalComponent = principalComponent;
            InterestComponent = interestComponent;
            OutstandingBalance = outstandingBalance;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks this installment as paid
        /// </summary>
        public void MarkAsPaid(decimal paidAmount, DateTime paidDate)
        {
            if (Status == "Paid")
                throw new DomainException($"Installment {InstallmentNumber} is already marked as paid.");
            
            PaidAmount = paidAmount;
            PaidDate = paidDate;
            Status = paidAmount >= EMIAmount ? "Paid" : "PartiallyPaid";
        }
    }
}
