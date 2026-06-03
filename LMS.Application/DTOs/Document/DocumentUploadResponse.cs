using System;

namespace LMS.Application.DTOs.Document
{
    public class DocumentUploadResponse
    {
        public Guid DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
