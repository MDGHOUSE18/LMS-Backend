using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Lookup;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Services.Loan
{
    public interface IDocumentService
    {
        Task<Document> UploadDocumentAsync(Guid loanId, Guid userId, DocumentType type, IFormFile file);
        Task<Document?> GetByIdAsync(Guid id);
        Task<List<Document>> GetDocumentsByLoanIdAsync(Guid loanId);
        Task<Document> VerifyDocumentAsync(Guid documentId, Guid verifiedByUserId, bool isApproved, string? rejectionReason = null);
        Task<List<Document>> GetPendingVerificationAsync();
        Task<bool> AreAllDocumentsVerifiedAsync(Guid loanId);
    }
}
