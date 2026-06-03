using System;

namespace LMS.Application.DTOs.Document
{
    public class VerifyDocumentRequest
    {
        public Guid DocumentId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
