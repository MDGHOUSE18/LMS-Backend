using LMS.Domain.Entities.Lookup;
using LMS.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class LoanApplication
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        
        // Status: Draft, Submitted, Under Review, Approved, Rejected, Disbursed
        public string Status { get; set; } = "Draft";
        
        public string? Purpose { get; set; } // Home, Auto, Personal, Education
        public string EmploymentType { get; set; } = default!; // Salaried, Self-employed
        
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public DateTime? DisbursedDate { get; set; }
        public string? RejectionReason { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        
        public Guid CreatedBy { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public Guid? ApprovedBy { get; set; }
        public Guid? RejectedBy { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual LoanFinancialDetails? FinancialDetails { get; set; }
        public virtual ICollection<Document>? Documents { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
        public virtual ICollection<LoanStatusHistory>? StatusHistories { get; set; }
    }
}
