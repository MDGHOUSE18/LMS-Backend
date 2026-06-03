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
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StatusId { get; set; }
        public int? EmploymentTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }
        public int? PurposeId { get; set; }

        // Navigation Properties
        public Auth.User User { get; set; } = null!;
        public LoanStatus Status { get; set; } = null!;
        public EmploymentType? EmploymentType { get; set; }
        public LoanPurpose? Purpose { get; set; }
        public LoanFinancialDetails? FinancialDetails { get; set; }
        public ICollection<LoanAssignment> Assignments { get; set; } = new List<LoanAssignment>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<EligibilityResult> EligibilityResults { get; set; } = new List<EligibilityResult>();
        public ICollection<LoanStatusHistory> StatusHistories { get; set; } = new List<LoanStatusHistory>();
    }
}
