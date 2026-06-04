using LMS.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class Document
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public Guid UserId { get; set; }
        
        public int DocumentTypeId { get; set; }
        public string Type { get; set; } // Aadhaar, PAN, SalarySlip, BankStatement
        public string FilePath { get; set; } // S3 path or storage path
        public string FileName { get; set; }
        public long FileSize { get; set; } // in bytes
        public string ContentType { get; set; } // application/pdf, image/jpeg
        
        public int VerificationStatus { get; set; } // Pending, Verified, Rejected
        public string? RejectionReason { get; set; }
        
        public DateTime UploadedAt { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        
        public DateTime? ExpiryDate { get; set; } // For Aadhaar (10 yrs), PAN (20 yrs)
        
        // Navigation properties
        public virtual LoanApplication? Loan { get; set; }
        public virtual User? User { get; set; }
        public virtual User? Verifier { get; set; }
    }
}
