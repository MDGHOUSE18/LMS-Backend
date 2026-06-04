using LMS.Application.DTOs.Document;
using LMS.Application.Interfaces.Common;
using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Lookup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ICurrentUserService _currentUserService;

        public DocumentsController(IDocumentService documentService, ICurrentUserService currentUserService)
        {
            _documentService = documentService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Upload KYC document for a loan application
        /// </summary>
        [HttpPost("upload/{loanId}")]
        [ProducesResponseType(typeof(DocumentUploadResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadDocument(Guid loanId, IFormFile file, [FromQuery] string documentType)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Only PDF and JPG files are allowed" });

                // Validate file size (max 5MB)
                const int maxFileSize = 5 * 1024 * 1024;
                if (file.Length > maxFileSize)
                    return BadRequest(new { message = "File size cannot exceed 5MB" });

                // Parse document type
                var docType = new DocumentType { TypeName = documentType };
                if (string.IsNullOrWhiteSpace(docType.TypeName))
                    return BadRequest(new { message = "Invalid document type. Valid types: Aadhaar, PAN, SalarySlip, BankStatement" });

                // Get current user ID
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var document = await _documentService.UploadDocumentAsync(loanId, userId, docType, file);

                var response = new DocumentUploadResponse
                {
                    DocumentId = document.Id,
                    DocumentType = document.Type.ToString(),
                    FileName = document.FileName,
                    FileSize = document.FileSize,
                    VerificationStatus = document.VerificationStatus.ToString(),
                    UploadedAt = document.UploadedAt
                };

                return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading document", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all documents for a loan
        /// </summary>
        [HttpGet("loan/{loanId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentsByLoan(Guid loanId)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByLoanIdAsync(loanId);
                var response = documents.Select(d => new DocumentUploadResponse
                {
                    DocumentId = d.Id,
                    DocumentType = d.Type.ToString(),
                    FileName = d.FileName,
                    FileSize = d.FileSize,
                    VerificationStatus = d.VerificationStatus.ToString(),
                    UploadedAt = d.UploadedAt
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Get document by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentUploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentById(Guid id)
        {
            try
            {
                var document = await _documentService.GetByIdAsync(id);
                if (document == null)
                    return NotFound(new { message = "Document not found" });

                var response = new DocumentUploadResponse
                {
                    DocumentId = document.Id,
                    DocumentType = document.Type.ToString(),
                    FileName = document.FileName,
                    FileSize = document.FileSize,
                    VerificationStatus = document.VerificationStatus.ToString(),
                    UploadedAt = document.UploadedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching document", error = ex.Message });
            }
        }

        /// <summary>
        /// Verify document (Officer/Admin only)
        /// </summary>
        [HttpPut("verify")]
        [Authorize(Roles = "Officer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> VerifyDocument([FromBody] VerifyDocumentRequest request)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var document = await _documentService.VerifyDocumentAsync(
                    request.DocumentId,
                    userId,
                    request.IsApproved,
                    request.RejectionReason
                );

                return Ok(new
                {
                    message = document.VerificationStatus == 2 
                        ? "Document verified successfully" 
                        : "Document rejected",
                    documentId = document.Id,
                    status = document.VerificationStatus.ToString()
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error verifying document", error = ex.Message });
            }
        }

        /// <summary>
        /// Get pending documents for verification (Officer/Admin only)
        /// </summary>
        [HttpGet("pending-verification")]
        [Authorize(Roles = "Officer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingVerifications()
        {
            try
            {
                var documents = await _documentService.GetPendingVerificationAsync();
                var response = documents.Select(d => new
                {
                    DocumentId = d.Id,
                    LoanId = d.LoanId,
                    DocumentType = d.Type.ToString(),
                    FileName = d.FileName,
                    UploadedAt = d.UploadedAt,
                    CustomerName = "Customer Name" // Need to join with User entity
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching pending verifications", error = ex.Message });
            }
        }
    }
}
