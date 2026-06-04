using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Lookup;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class DocumentService : LMS.Application.Interfaces.Services.Loan.IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILoanRepository _loanRepository;

        public DocumentService(IDocumentRepository documentRepository, ILoanRepository loanRepository)
        {
            _documentRepository = documentRepository;
            _loanRepository = loanRepository;
        }

        public async Task<Document> UploadDocumentAsync(Guid loanId, Guid userId, DocumentType type, IFormFile file)
        {
            // Validate file
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // Validate file type (PDF or JPG only)
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Only PDF and JPG files are allowed");

            // Validate file size (max 5MB)
            const int maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
                throw new ArgumentException("File size cannot exceed 5MB");

            // Verify loan exists
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";

            // In production, upload to S3/Azure Blob Storage
            // For now, we'll store metadata only (actual file storage to be implemented)
            var s3Path = $"/documents/{loanId}/{fileName}";

            var document = new Document
            {
                Id = Guid.NewGuid(),
                LoanId = loanId,
                UserId = userId,
                Type = type.TypeName,
                FileName = file.FileName,
                FilePath = s3Path,
                FileSize = file.Length,
                VerificationStatus = 1,
                UploadedAt = DateTime.UtcNow
            };

            _documentRepository.Add(document);

            return document;
        }

        public async Task<List<Document>> GetDocumentsByLoanIdAsync(Guid loanId)
        {
            return await _documentRepository.GetByLoanIdAsync(loanId);
        }

        public async Task<Document?> GetByIdAsync(Guid id)
        {
            return await _documentRepository.GetByIdAsync(id);
        }

        public async Task<Document> VerifyDocumentAsync(Guid documentId, Guid verifiedByUserId, bool isApproved, string? rejectionReason = null)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                throw new ArgumentException("Document not found");

            if (document.VerificationStatus != 1)
                throw new InvalidOperationException("Document already verified");

            document.VerificationStatus = isApproved ? 2 : 3;
            document.VerifiedBy = verifiedByUserId;
            document.VerifiedAt = DateTime.UtcNow;
            document.RejectionReason = rejectionReason;

            _documentRepository.Update(document);
            

            return document;
        }

        public async Task<List<Document>> GetPendingVerificationAsync()
        {
            return await _documentRepository.GetPendingVerificationAsync();
        }

        public async Task<bool> AreAllDocumentsVerifiedAsync(Guid loanId)
        {
            var documents = await _documentRepository.GetByLoanIdAsync(loanId);

            // Check if all required documents are present and verified
            var requiredTypes = new[]
                {
                    "Aadhaar",
                    "PAN",
                    "Salary Slip"
                };

            foreach (var requiredType in requiredTypes)
            {
                var doc = documents.FirstOrDefault(
                        d => d.Type == requiredType
                    );
                if (doc == null || doc.VerificationStatus != 2)
                    return false;
            }

            return true;
        }
    }
}
