using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class Document
    {
        public int Id { get; set; }                      
        public int LoanApplicationId { get; set; }
        public int DocumentTypeId { get; set; }
        public int? VerificationStatusId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int FileSize { get; set; }
        public int UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }

        // Navigation Properties
        public LoanApplication LoanApplication { get; set; } = null!;
        public DocumentType DocumentType { get; set; } = null!;
        public VerificationStatus? VerificationStatus { get; set; }
        public Auth.User UploadedByUser { get; set; } = null!;
    }
}
