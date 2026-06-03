using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Core.DTOs;
using LoanManagementSystem.Core.Entities;
using LoanManagementSystem.Core.Interfaces;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/loans/{loanId}/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public DocumentsController(
            IDocumentRepository documentRepository,
            ILoanRepository loanRepository,
            IAuditLogRepository auditLogRepository)
        {
            _documentRepository = documentRepository;
            _loanRepository = loanRepository;
            _auditLogRepository = auditLogRepository;
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(Guid loanId, [FromForm] string type, IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Validate loan exists and user has access
                var loan = await _loanRepository.GetWithDetailsAsync(loanId);
                if (loan == null)
                    return NotFound(new { message = "Loan not found" });

                if (loan.UserId != userId)
                    return Forbid();

                // Parse document type
                if (!Enum.TryParse<DocumentType>(type, true, out var docType))
                    return BadRequest(new { message = "Invalid document type. Valid types: Aadhaar, PAN, SalarySlip, BankStatement" });

                // Validate file
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Please upload PDF or JPG file only" });

                if (file.Length > 5 * 1024 * 1024) // 5MB
                    return BadRequest(new { message = "File size must not exceed 5MB" });

                // Create document record
                var document = new Document
                {
                    DocId = Guid.NewGuid(),
                    LoanId = loanId,
                    UserId = userId,
                    Type = docType,
                    S3Path = $"/documents/{loanId}/{docType}_{Guid.NewGuid()}{extension}", // Mock path
                    FileName = file.FileName,
                    VerificationStatus = VerificationStatus.Pending,
                    UploadedAt = DateTime.UtcNow
                };

                await _documentRepository.AddAsync(document);
                await CreateAuditLog(userId, "Upload", "Document", document.DocId, $"Document uploaded: {docType}");

                var response = new DocumentResponseDto
                {
                    DocId = document.DocId,
                    LoanId = document.LoanId,
                    Type = document.Type,
                    VerificationStatus = document.VerificationStatus,
                    UploadedAt = document.UploadedAt
                };

                return CreatedAtAction(nameof(GetDocumentById), new { loanId, id = document.DocId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Document upload failed", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(Guid loanId, Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var document = await _documentRepository.GetByIdAsync(id);
                
                if (document == null || document.LoanId != loanId)
                    return NotFound(new { message = "Document not found" });

                var loan = await _loanRepository.GetWithDetailsAsync(loanId);
                if (loan.UserId != userId)
                    return Forbid();

                var response = new DocumentResponseDto
                {
                    DocId = document.DocId,
                    LoanId = document.LoanId,
                    Type = document.Type,
                    VerificationStatus = document.VerificationStatus,
                    UploadedAt = document.UploadedAt,
                    VerifiedAt = document.VerifiedAt,
                    RejectionReason = document.RejectionReason
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch document", error = ex.Message });
            }
        }

        [HttpPut("{id}/verify")]
        [Authorize(Roles = "Officer,Admin")]
        public async Task<IActionResult> VerifyDocument(Guid loanId, Guid id, [FromBody] VerifyDocumentDto dto)
        {
            try
            {
                var officerId = GetCurrentUserId();
                var document = await _documentRepository.GetByIdAsync(id);
                
                if (document == null || document.LoanId != loanId)
                    return NotFound(new { message = "Document not found" });

                if (dto.IsVerified)
                {
                    document.VerificationStatus = VerificationStatus.Verified;
                    document.VerifiedBy = officerId;
                    document.VerifiedAt = DateTime.UtcNow;
                }
                else
                {
                    document.VerificationStatus = VerificationStatus.Rejected;
                    document.VerifiedBy = officerId;
                    document.VerifiedAt = DateTime.UtcNow;
                    document.RejectionReason = dto.RejectionReason ?? "Document verification failed";
                }

                await _documentRepository.UpdateAsync(document);
                await CreateAuditLog(officerId, dto.IsVerified ? "Verify" : "Reject", "Document", document.DocId, 
                    dto.IsVerified ? "Document verified" : $"Document rejected: {dto.RejectionReason}");

                return Ok(new { 
                    message = dto.IsVerified ? "Document verified successfully" : "Document rejected",
                    status = document.VerificationStatus 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Verification failed", error = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("User not authenticated");
            return userId;
        }

        private async Task CreateAuditLog(Guid? userId, string action, string entityType, Guid entityId, string details)
        {
            var auditLog = new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            await _auditLogRepository.AddAsync(auditLog);
        }
    }

    public class VerifyDocumentDto
    {
        public bool IsVerified { get; set; }
        public string? RejectionReason { get; set; }
    }
}
